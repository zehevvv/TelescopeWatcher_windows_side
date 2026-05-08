using System.Net.Http;
using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class VideoPlayerForm : Form
    {
        private readonly string serverBaseUrl;
        private readonly string primaryCameraName;
        private readonly string secondaryCameraName;
        private readonly string mjpegUrl1;
        private readonly string mjpegUrl2;
        private int totalFrameCount1 = 0;
        private int totalFrameCount2 = 0;
        private int frameCount1 = 0;
        private int frameCount2 = 0;
        private long lastFpsUpdate1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private long lastFpsUpdate2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private bool mainFlipHorizontal = true;
        private bool mainFlipVertical = true;
        private bool secFlipHorizontal = true;
        private bool secFlipVertical = true;

        // Circle overlay fields
        private bool isAddingCircle = false;
        private Point? whiteCirclePosition = null;
        private PointF? whiteCirclePositionRelative = null;
        private int whiteCircleRadius = 30;
        private int circleRadius = 30;
        private const int MIN_RADIUS = 10;
        private const int MAX_RADIUS = 200;
        private Point currentMousePosition;

        // Telescope control fields
        private TelescopeController telescopeController;
        private MjpegStreamClient mjpegClient1;
        private MjpegStreamClient mjpegClient2;
        
        private bool isKeyPressed = false;
        private bool isFocusKeyPressed = false;
        private string currentDirection = "";
        private string currentFocusDirection = "";
        private System.Windows.Forms.Timer commandTimer;
        private System.Windows.Forms.Timer focusTimer;
        private System.Windows.Forms.Timer fpsTimer;

        // Latest Frame Synchronization
        private readonly object _lock1 = new object();
        private Image? _pendingFrame1 = null;
        private bool _updatePending1 = false;

        private readonly object _lock2 = new object();
        private Image? _pendingFrame2 = null;
        private bool _updatePending2 = false;

        private Action<string>? logCallback;

        public VideoPlayerForm(string serverUrl, string primaryCamera, string primaryStreamUrl,
                               string secondaryCamera, string secondaryStreamUrl,
                               SerialPort? port = null, SerialServerClient? client = null,
                               int stepsPerSecond = 1000, int focusMotorSpeed = 9, Action<string>? logCallback = null)
        {
            this.serverBaseUrl = serverUrl;
            this.primaryCameraName = primaryCamera;
            this.secondaryCameraName = secondaryCamera;
            this.mjpegUrl1 = primaryStreamUrl;
            this.mjpegUrl2 = secondaryStreamUrl;
            
            this.logCallback = logCallback;
            
            // Initialize Helpers
            this.telescopeController = new TelescopeController(port, client, logCallback);
            
            this.mjpegClient1 = new MjpegStreamClient();
            this.mjpegClient1.FrameReceived += MjpegClient1_FrameReceived;
            this.mjpegClient1.FlipHorizontal = mainFlipHorizontal;
            this.mjpegClient1.FlipVertical = mainFlipVertical;

            this.mjpegClient2 = new MjpegStreamClient();
            this.mjpegClient2.FrameReceived += MjpegClient2_FrameReceived;
            this.mjpegClient2.FlipHorizontal = secFlipHorizontal;
            this.mjpegClient2.FlipVertical = secFlipVertical;
            
            InitializeComponent();

            // Update title with camera names
            this.Text = $"Video Stream - Primary: {primaryCamera.ToUpper()}, Guide: {secondaryCamera.ToUpper()}";

            // Use shared settings via Controller
            var settings = TelescopeSettings.Instance;
            telescopeController.TimeBetweenSteps = settings.TimeBetweenSteps;
            telescopeController.FocusSpeed = settings.FocusSpeed;
            
            // Subscribe to settings changes AFTER InitializeComponent
            settings.StepsPerSecondChanged += OnStepsPerSecondChanged;
            settings.FocusSpeedChanged += OnFocusSpeedChanged;
            
            // Force default to 1000 and update trackbar/settings
            settings.StepsPerSecond = 1000;
            // Explicitly set trackbar value to match 1000 (Index 3)
            // Indices: 0=3, 1=10, 2=100, 3=1000, 4=10000, 5=100000
            if (trackBarStepsPerSecond != null) trackBarStepsPerSecond.Value = 3;

            settings.FocusSpeed = focusMotorSpeed;

            if (this.videoPanel != null)
            {
                this.videoPanel.Visible = true;
            }

            // FIX: Anchor controls to Top|Right so they stay on the right side
            if (btnCircleSizeIncrease != null) btnCircleSizeIncrease.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (lblCircleSize != null) lblCircleSize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (btnCircleSizeDecrease != null) btnCircleSizeDecrease.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (btnAddCircle != null) btnAddCircle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            
            if (lblFocusSpeedValue != null) lblFocusSpeedValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (trackBarFocusSpeed != null) trackBarFocusSpeed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (lblFocusSpeed != null) lblFocusSpeed.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Initialize display labels
            UpdateStepsPerSecondDisplay();
            UpdateFocusSpeedDisplay();
            
            // Initial layout
            PositionCircleControls();
            PositionFocusControls();

            this.FormClosing += VideoPlayerForm_FormClosing;
            LoadWhiteCirclePosition();
            
            commandTimer = new System.Windows.Forms.Timer();
            commandTimer.Interval = 200;
            commandTimer.Tick += CommandTimer_Tick;

            focusTimer = new System.Windows.Forms.Timer();
            focusTimer.Interval = 100;
            focusTimer.Tick += FocusTimer_Tick;

            // Initialize FPS timer
            fpsTimer = new System.Windows.Forms.Timer();
            fpsTimer.Interval = 1000;
            fpsTimer.Tick += FpsTimer_Tick;
            fpsTimer.Start();
            
            // Force layout update for split screen
            VideoPlayerForm_Resize(this, EventArgs.Empty);
            // Verify Z-order and docking state
            if (radioBoth != null && radioBoth.Checked) RadioStream_CheckedChanged(radioBoth, EventArgs.Empty);
        }

        private void OnStepsPerSecondChanged(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnStepsPerSecondChanged(sender, e)));
                return;
            }
            
            var settings = TelescopeSettings.Instance;
            telescopeController.TimeBetweenSteps = settings.TimeBetweenSteps;
            
            // Update trackbar without triggering event
            trackBarStepsPerSecond.ValueChanged -= TrackBarStepsPerSecond_ValueChanged;
            trackBarStepsPerSecond.Value = settings.GetTrackbarIndexForStepsPerSecond();
            trackBarStepsPerSecond.ValueChanged += TrackBarStepsPerSecond_ValueChanged;
            
            UpdateStepsPerSecondDisplay();
        }

        private void OnFocusSpeedChanged(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnFocusSpeedChanged(sender, e)));
                return;
            }
            
            var settings = TelescopeSettings.Instance;
            telescopeController.FocusSpeed = settings.FocusSpeed;
            
            // Update trackbar without triggering event
            trackBarFocusSpeed.ValueChanged -= TrackBarFocusSpeed_ValueChanged;
            trackBarFocusSpeed.Value = settings.FocusSpeed;
            trackBarFocusSpeed.ValueChanged += TrackBarFocusSpeed_ValueChanged;
            
            UpdateFocusSpeedDisplay();
        }

        private void TrackBarStepsPerSecond_ValueChanged(object? sender, EventArgs e)
        {
            var settings = TelescopeSettings.Instance;
            settings.SetStepsPerSecondFromTrackbarIndex(trackBarStepsPerSecond.Value);
            UpdateStepsPerSecondDisplay();
        }

        private void TrackBarFocusSpeed_ValueChanged(object? sender, EventArgs e)
        {
            var settings = TelescopeSettings.Instance;
            settings.FocusSpeed = trackBarFocusSpeed.Value;
            UpdateFocusSpeedDisplay();
        }

        private void UpdateStepsPerSecondDisplay()
        {
            var settings = TelescopeSettings.Instance;
            int stepsPerSecond = settings.StepsPerSecond;
            double timeMs = stepsPerSecond == 100000 ? 0.01 : (stepsPerSecond == 10000 ? 0.1 : 1000.0 / stepsPerSecond);
            string timeFmt = stepsPerSecond == 100000 ? "0.01" : timeMs.ToString("F1");
            lblStepsPerSecondValue.Text = $"{stepsPerSecond} steps/sec (t={timeFmt}ms)";
        }

        private void UpdateFocusSpeedDisplay()
        {
            var settings = TelescopeSettings.Instance;
            lblFocusSpeedValue.Text = $"Speed: {settings.FocusSpeed}";
        }

        private void PositionCircleControls()
        {
            if (controlPanel == null) return;
            
            int rightMargin = 2; // Reduced from 10 to 2
            int yPos = 3;
            
            // Position from right to left: + button, label, - button, Add Circle button
            if (btnCircleSizeIncrease != null)
                btnCircleSizeIncrease.Location = new System.Drawing.Point(controlPanel.Width - rightMargin - btnCircleSizeIncrease.Width, yPos);
            
            if (lblCircleSize != null && btnCircleSizeIncrease != null)
                lblCircleSize.Location = new System.Drawing.Point(btnCircleSizeIncrease.Left - 2 - lblCircleSize.Width, yPos + 5);
            
            if (btnCircleSizeDecrease != null && lblCircleSize != null)
                btnCircleSizeDecrease.Location = new System.Drawing.Point(lblCircleSize.Left - 2 - btnCircleSizeDecrease.Width, yPos);
            
            if (btnAddCircle != null && btnCircleSizeDecrease != null)
                btnAddCircle.Location = new System.Drawing.Point(btnCircleSizeDecrease.Left - 5 - btnAddCircle.Width, yPos + 2);
        }

        private void PositionFocusControls()
        {
            if (telescopeControlPanel == null) return;
            
            int rightMargin = 2; // Reduced from 10 to 2
            int labelYPos = 10;
            int trackbarYPos = 30;
            int valueYPos = 35;
            
            // Position focus speed value label from the right (flushed)
            if (lblFocusSpeedValue != null)
                lblFocusSpeedValue.Location = new System.Drawing.Point(
                    telescopeControlPanel.Width - rightMargin - lblFocusSpeedValue.PreferredWidth, 
                    valueYPos
                );
            
            // Position trackbar to the left of the value label
            if (trackBarFocusSpeed != null && lblFocusSpeedValue != null)
                trackBarFocusSpeed.Location = new System.Drawing.Point(
                    lblFocusSpeedValue.Left - 5 - trackBarFocusSpeed.Width, 
                    trackbarYPos
                );
            
            // Position the label above the trackbar
            if (lblFocusSpeed != null && trackBarFocusSpeed != null)
                lblFocusSpeed.Location = new System.Drawing.Point(
                    trackBarFocusSpeed.Left, 
                    labelYPos
                );
        }

        private void PositionSaveFrameButton()
        {
            if (telescopeControlPanel == null || btnSaveFrame == null) return;

            // Center the save frame button
            int startX = (telescopeControlPanel.Width - btnSaveFrame.Width) / 2;

            btnSaveFrame.Location = new Point(startX, 35);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // WM_KEYDOWN = 0x100, WM_KEYUP = 0x101
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;
            
            // Check if this is an arrow key or Page key
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right ||
                key == Keys.PageUp || key == Keys.PageDown)
            {
                if (msg.Msg == WM_KEYDOWN)
                {
                    var args = new KeyEventArgs(keyData);
                    VideoPlayerForm_KeyDown(this, args);
                }
                else if (msg.Msg == WM_KEYUP)
                {
                    var args = new KeyEventArgs(keyData);
                    VideoPlayerForm_KeyUp(this, args);
                }
                return true; // Prevent default behavior (radio button navigation)
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void VideoPlayerForm_Resize(object? sender, EventArgs e)
        {
            if (radioBoth != null && radioBoth.Checked && pictureBox2 != null && videoPanel != null)
            {
                // Maintain 50% split on resize
                pictureBox2.Width = videoPanel.ClientSize.Width / 2;
            }
            
            // Force re-layout
            PositionCircleControls();
            PositionFocusControls();
            PositionSaveFrameButton();
            
            UpdateWhiteCircleAbsolutePosition();
            if (pictureBox2 != null) pictureBox2.Invalidate();
            if (pictureBox1 != null) pictureBox1.Invalidate();
        }

        private void RadioStream_CheckedChanged(object? sender, EventArgs e)
        {
            // Ensure Panels are sent to Back
            if (controlPanel != null) controlPanel.SendToBack();
            if (telescopeControlPanel != null) telescopeControlPanel.SendToBack();
            if (lblStatus != null) lblStatus.SendToBack();
            if (btnClose != null) btnClose.SendToBack();
            if (lblFrameInfo1 != null) lblFrameInfo1.SendToBack();
            if (lblFrameInfo2 != null) lblFrameInfo2.SendToBack();

            if (pictureBox1 == null || pictureBox2 == null) return;

            if (radioMainOnly.Checked)
            {
                // Main Only
                pictureBox2.Visible = false;
                
                pictureBox1.Visible = true;
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox1.BringToFront(); 
                
                if (lblFrameInfo1 != null) lblFrameInfo1.Visible = true;
                if (lblFrameInfo2 != null) lblFrameInfo2.Visible = false;
            }
            else if (radioSecondaryOnly.Checked)
            {
                // Secondary Only
                pictureBox1.Visible = false;
                
                pictureBox2.Visible = true;
                pictureBox2.Dock = DockStyle.Fill;
                pictureBox2.BringToFront(); 
                
                if (lblFrameInfo1 != null) lblFrameInfo1.Visible = false;
                if (lblFrameInfo2 != null) lblFrameInfo2.Visible = true;
            }
            else if (radioBoth.Checked)
            {
                // Both: Split View
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                
                if (lblFrameInfo1 != null) lblFrameInfo1.Visible = true;
                if (lblFrameInfo2 != null) lblFrameInfo2.Visible = true;

                // 1. Setup PicBox2 (Right) - docked inside videoPanel
                pictureBox2.Dock = DockStyle.Right;
                pictureBox2.Width = videoPanel.ClientSize.Width / 2;
                pictureBox2.BringToFront(); 

                // 2. Setup PicBox1 (Fill remaining space inside videoPanel)
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox1.BringToFront(); 
            }
            
            UpdateWhiteCircleAbsolutePosition();
            pictureBox2.Invalidate();
        }

        private async void VideoPlayerForm_Load(object? sender, EventArgs e)
        {
            LoadWhiteCirclePosition();
            await StartStreaming();
        }

        private async Task StartStreaming()
        {
            try
            {
                UpdateStatus("Connecting to streams...", System.Drawing.Color.DarkOrange);
                
                // Start Stream 1 (Main)
                mjpegClient1.FlipHorizontal = mainFlipHorizontal;
                mjpegClient1.FlipVertical = mainFlipVertical;
                var task1 = mjpegClient1.StartStream(mjpegUrl1, 1);

                // Start Stream 2 (Secondary)
                mjpegClient2.FlipHorizontal = secFlipHorizontal;
                mjpegClient2.FlipVertical = secFlipVertical;
                var task2 = mjpegClient2.StartStream(mjpegUrl2, 2);

                await Task.WhenAll(task1, task2); // Wait for initialization logic if any (StartStream mostly async void/Task)

                UpdateStatus("Streams connected", System.Drawing.Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start video streams:\n\n{ex.Message}", 
                    "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void MjpegClient1_FrameReceived(object? sender, Image image)
        {
            // Always overwrite pending frame to ensure latest is used
            lock(_lock1)
            {
                if (_pendingFrame1 != null)
                {
                    _pendingFrame1.Dispose(); // Discard previous pending frame
                }
                _pendingFrame1 = image;

                if (!_updatePending1)
                {
                    _updatePending1 = true;
                    this.BeginInvoke(new Action(ProcessPendingFrame1));
                }
            }
        }

        private void ProcessPendingFrame1()
        {
            Image? frameToRender = null;
            
            lock (_lock1)
            {
                frameToRender = _pendingFrame1;
                _pendingFrame1 = null;
                
                if (frameToRender == null)
                {
                    _updatePending1 = false;
                    return;
                }
            }

            try
            {
                UpdateImage(frameToRender, 1);

                totalFrameCount1++;
                frameCount1++;
                var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastFpsUpdate1;
                if (elapsed >= 1000)
                {
                    UpdateFrameInfo(totalFrameCount1, frameCount1, 1);
                    lastFpsUpdate1 += 1000;
                    frameCount1 = 0;
                }                                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering frame 1: {ex.Message}");
            }
            finally
            {
                // Check if more frames arrived while we were rendering
                lock (_lock1)
                {
                    if (_pendingFrame1 != null)
                    {
                        // Schedule next update immediately
                        this.BeginInvoke(new Action(ProcessPendingFrame1));
                    }
                    else
                    {
                        _updatePending1 = false;
                    }
                }
            }
        }

        private void MjpegClient2_FrameReceived(object? sender, Image image)
        {
            // Always overwrite pending frame to ensure latest is used
            lock(_lock2)
            {
                if (_pendingFrame2 != null)
                {
                    _pendingFrame2.Dispose(); // Discard previous pending frame
                }
                _pendingFrame2 = image;

                if (!_updatePending2)
                {
                    _updatePending2 = true;
                    this.BeginInvoke(new Action(ProcessPendingFrame2));
                }
            }
        }

        private void ProcessPendingFrame2()
        {
            Image? frameToRender = null;
            
            lock (_lock2)
            {
                frameToRender = _pendingFrame2;
                _pendingFrame2 = null;
                
                if (frameToRender == null)
                {
                    _updatePending2 = false;
                    return;
                }
            }

            try
            {
                UpdateImage(frameToRender, 2);

                totalFrameCount2++;
                frameCount2++;
                var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastFpsUpdate2;
                if (elapsed >= 1000)
                {
                    UpdateFrameInfo(totalFrameCount2, frameCount2, 2);
                    lastFpsUpdate2 += 1000;
                    frameCount2 = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering frame 2: {ex.Message}");
            }
            finally
            {
                // Check if more frames arrived while we were rendering
                lock (_lock2)
                {
                    if (_pendingFrame2 != null)
                    {
                        // Schedule next update immediately
                        this.BeginInvoke(new Action(ProcessPendingFrame2));
                    }
                    else
                    {
                        _updatePending2 = false;
                    }
                }
            }
        }

        // Renamed/Unified handler doesn't work well with event sig unless we wrap. 
        // Kept separate handlers above for simplicity.

        private void UpdateImage(Image image, int streamId)
        {
            PictureBox? targetBox = streamId == 1 ? pictureBox1 : pictureBox2;
            if (targetBox == null) 
            {
                image.Dispose();
                return;
            }
            
            // This method is now always called on UI thread via ProcessPendingFrame
            
            var oldImage = targetBox.Image;
            bool wasFirstFrame = (oldImage == null);
            
            targetBox.Image = image;
            oldImage?.Dispose();
            
            if (wasFirstFrame && streamId == 2 && whiteCirclePositionRelative.HasValue) // Circles only on PicBox2 for now? Logic says PictureBox2_Paint usage.
            {
                UpdateWhiteCircleAbsolutePosition();
                targetBox.Invalidate();
            }
        }

        private void UpdateStatus(string text, System.Drawing.Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action<string, System.Drawing.Color>(UpdateStatus), text, color);
                return;
            }

            lblStatus.Text = text;
            lblStatus.BackColor = color;
        }

        private void UpdateFrameInfo(int frames, double fps, int streamId)
        {
            var label = streamId == 1 ? lblFrameInfo1 : lblFrameInfo2;
            string newText;
            
            if (streamId == 1)
            {
                newText = $"Main: Frame {frames} | FPS: {fps:F1}";
            }
            else
            {
                newText = $"Secondary: Frame {frames} | FPS: {fps:F1}";
            }
            
            if (label != null) // Check null
            {
                if (label.InvokeRequired)
                {
                    label.Invoke(new Action<int, double, int>(UpdateFrameInfo), frames, fps, streamId);
                    return;
                }

                if (label.Text != newText)
                {
                    label.Text = newText;
                }
            }
        }

        private void FpsTimer_Tick(object? sender, EventArgs e)
        {
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void VideoPlayerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopStreaming();
            
            commandTimer?.Stop();
            commandTimer?.Dispose();
            focusTimer?.Stop();
            focusTimer?.Dispose();
            fpsTimer?.Stop();
            fpsTimer?.Dispose();
            
            mjpegClient1?.Dispose();
            mjpegClient2?.Dispose();
        }

        private void StopStreaming()
        {
            mjpegClient1?.StopStreaming();
            mjpegClient2?.StopStreaming();

            pictureBox1?.Image?.Dispose();
            pictureBox2?.Image?.Dispose();
        }

        private void ChkMainFlipH_CheckedChanged(object? sender, EventArgs e)
        {
            mainFlipHorizontal = chkMainFlipH.Checked;
            if (mjpegClient1 != null) mjpegClient1.FlipHorizontal = mainFlipHorizontal;
            System.Diagnostics.Debug.WriteLine($"Main Flip Horizontal: {mainFlipHorizontal}");
        }

        private void ChkMainFlipV_CheckedChanged(object? sender, EventArgs e)
        {
            mainFlipVertical = chkMainFlipV.Checked;
            if (mjpegClient1 != null) mjpegClient1.FlipVertical = mainFlipVertical;
            System.Diagnostics.Debug.WriteLine($"Main Flip Vertical: {mainFlipVertical}");
        }

        private void ChkSecFlipH_CheckedChanged(object? sender, EventArgs e)
        {
            secFlipHorizontal = chkSecFlipH.Checked;
            if (mjpegClient2 != null) mjpegClient2.FlipHorizontal = secFlipHorizontal;
            System.Diagnostics.Debug.WriteLine($"Sec Flip Horizontal: {secFlipHorizontal}");
        }

        private void ChkSecFlipV_CheckedChanged(object? sender, EventArgs e)
        {
            secFlipVertical = chkSecFlipV.Checked;
            if (mjpegClient2 != null) mjpegClient2.FlipVertical = secFlipVertical;
            System.Diagnostics.Debug.WriteLine($"Sec Flip Vertical: {secFlipVertical}");
        }

        private async void BtnSaveFrame_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1 != null && pictureBox1.Image != null)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg";
                        sfd.Title = "Save Current Main Camera Frame";
                        sfd.FileName = $"MainCamera_Frame_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            var format = sfd.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) 
                                ? System.Drawing.Imaging.ImageFormat.Jpeg 
                                : System.Drawing.Imaging.ImageFormat.Png;

                            // Save from PictureBox image
                            pictureBox1.Image.Save(sfd.FileName, format);
                            
                            LogMessage($"Frame saved to {sfd.FileName}");
                            UpdateStatus("Frame saved to file", System.Drawing.Color.DarkGreen);
                            
                            await Task.Delay(2000);
                            UpdateStatus("Streams connected", System.Drawing.Color.DarkGreen);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Main camera stream is not ready or has no image.", "Capture Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error capturing frame: {ex.Message}");
                MessageBox.Show($"Failed to save frame: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCalibration_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new CalibrationForm(serverBaseUrl);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening calibration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlateSolverToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new PlateSolverForm(serverBaseUrl);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening plate solver: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StarFollowerToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new StarFollowerForm(serverBaseUrl);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening star follower: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StarFollower2ToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new StarFollower2Form(telescopeController, mjpegUrl1, mjpegUrl2);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Star Follower 2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StarFollower3ToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new StarFollower3Form(telescopeController, mjpegUrl1, mjpegUrl2);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Star Follower 3: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SiderealTrackerToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                var form = new SiderealTrackerForm(serverBaseUrl);
                form.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening sidereal tracker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Keyboard Control Methods

        private void VideoPlayerForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (isKeyPressed || isFocusKeyPressed)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                isKeyPressed = true;
                currentDirection = "UP";
                telescopeController.SendMoveCommand("UP");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("UP arrow key pressed");
            }
            else if (e.KeyCode == Keys.Down)
            {
                isKeyPressed = true;
                currentDirection = "DOWN";
                telescopeController.SendMoveCommand("DOWN");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("DOWN arrow key pressed");
            }
            else if (e.KeyCode == Keys.Left)
            {
                isKeyPressed = true;
                currentDirection = "LEFT";
                telescopeController.SendMoveCommand("LEFT");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("LEFT arrow key pressed");
            }
            else if (e.KeyCode == Keys.Right)
            {
                isKeyPressed = true;
                currentDirection = "RIGHT";
                telescopeController.SendMoveCommand("RIGHT");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("RIGHT arrow key pressed");
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                isFocusKeyPressed = true;
                currentFocusDirection = "INCREASE";
                telescopeController.SendFocusCommand("INCREASE");
                focusTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("PageUp key pressed - Focus increase");
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                isFocusKeyPressed = true;
                currentFocusDirection = "DECREASE";
                telescopeController.SendFocusCommand("DECREASE");
                focusTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                LogMessage("PageDown key pressed - Focus decrease");
            }
        }

        private void VideoPlayerForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                isKeyPressed = false;
                commandTimer.Stop();
                telescopeController.SendStopCommand();
                string keyName = e.KeyCode == Keys.Up ? "UP" :
                                 e.KeyCode == Keys.Down ? "DOWN" :
                                 e.KeyCode == Keys.Left ? "LEFT" : "RIGHT";
                LogMessage($"{keyName} arrow key released - stopped sending commands");
                currentDirection = "";
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                isFocusKeyPressed = false;
                focusTimer.Stop();
                telescopeController.SendFocusStopCommand();
                string keyName = e.KeyCode == Keys.PageUp ? "PageUp" : "PageDown";
                LogMessage($"{keyName} key released - stopped focus commands");
                currentFocusDirection = "";
                e.Handled = true;
            }
        }

        private void CommandTimer_Tick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentDirection))
            {
                telescopeController.SendStepsCommand();
            }
        }

        private void FocusTimer_Tick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFocusDirection))
            {
                telescopeController.SendFocusStepsCommand();
            }
        }

        private void LogMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[VideoPlayerForm] {message}");
            logCallback?.Invoke(message);
        }

        #endregion

        private void LoadWhiteCirclePosition()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "circle_position.txt");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    if (lines.Length >= 3)
                    {
                        float relativeX = float.Parse(lines[0]);
                        float relativeY = float.Parse(lines[1]);
                        int radius = int.Parse(lines[2]);
                        
                        whiteCirclePositionRelative = new PointF(relativeX, relativeY);
                        whiteCircleRadius = radius;
                        
                        UpdateWhiteCircleAbsolutePosition();
                        
                        System.Diagnostics.Debug.WriteLine($"Loaded white circle relative position: ({relativeX:P1}, {relativeY:P1}) with radius: {whiteCircleRadius}");
                        
                        if (pictureBox2 != null)
                        {
                            pictureBox2.Invalidate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading white circle position: {ex.Message}");
            }
        }

        private void UpdateWhiteCircleAbsolutePosition()
        {
            if (whiteCirclePositionRelative.HasValue && pictureBox2 != null && pictureBox2.Image != null)
            {
                Rectangle displayRect = GetImageDisplayRectangle(pictureBox2);
                
                if (displayRect.Width > 0 && displayRect.Height > 0)
                {
                    int absoluteX = displayRect.X + (int)(whiteCirclePositionRelative.Value.X * displayRect.Width);
                    int absoluteY = displayRect.Y + (int)(whiteCirclePositionRelative.Value.Y * displayRect.Height);
                    whiteCirclePosition = new Point(absoluteX, absoluteY);
                    System.Diagnostics.Debug.WriteLine($"Updated absolute position: {whiteCirclePosition} from relative ({whiteCirclePositionRelative.Value.X:P1}, {whiteCirclePositionRelative.Value.Y:P1}) for display rect {displayRect}");
                }
            }
        }

        private void SaveWhiteCirclePosition()
        {
            try
            {
                if (whiteCirclePositionRelative.HasValue)
                {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "circle_position.txt");
                    File.WriteAllLines(configPath, new[]
                    {
                        whiteCirclePositionRelative.Value.X.ToString("F6"),
                        whiteCirclePositionRelative.Value.Y.ToString("F6"),
                        whiteCircleRadius.ToString()
                    });
                    System.Diagnostics.Debug.WriteLine($"Saved white circle relative position: ({whiteCirclePositionRelative.Value.X:P1}, {whiteCirclePositionRelative.Value.Y:P1}) with radius: {whiteCircleRadius}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving white circle position: {ex.Message}");
            }
        }

        private Rectangle GetImageDisplayRectangle(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
                return Rectangle.Empty;

            float imageAspect = (float)pictureBox.Image.Width / pictureBox.Image.Height;
            float containerAspect = (float)pictureBox.Width / pictureBox.Height;

            int displayWidth, displayHeight, displayX, displayY;

            if (imageAspect > containerAspect)
            {
                displayWidth = pictureBox.Width;
                displayHeight = (int)(pictureBox.Width / imageAspect);
                displayX = 0;
                displayY = (pictureBox.Height - displayHeight) / 2;
            }
            else
            {
                displayHeight = pictureBox.Height;
                displayWidth = (int)(pictureBox.Height * imageAspect);
                displayX = (pictureBox.Width - displayWidth) / 2;
                displayY = 0;
            }

            return new Rectangle(displayX, displayY, displayWidth, displayHeight);
        }

        private void PictureBox2_Paint(object? sender, PaintEventArgs e)
        {
            if (whiteCirclePosition.HasValue)
            {
                using (Pen whitePen = new Pen(Color.White, 2))
                {
                    int x = whiteCirclePosition.Value.X - whiteCircleRadius;
                    int y = whiteCirclePosition.Value.Y - whiteCircleRadius;
                    e.Graphics.DrawEllipse(whitePen, x, y, whiteCircleRadius * 2, whiteCircleRadius * 2);
                }
            }

            if (isAddingCircle)
            {
                using (Pen redPen = new Pen(Color.Red, 2))
                {
                    int x = currentMousePosition.X - circleRadius;
                    int y = currentMousePosition.Y - circleRadius;
                    e.Graphics.DrawEllipse(redPen, x, y, circleRadius * 2, circleRadius * 2);
                }
            }
        }

        private void BtnMainCameraControl_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsForm = new CameraSettingsForm(serverBaseUrl, primaryCameraName);
                settingsForm.Show();
                LogMessage($"Opening Primary Camera ({primaryCameraName.ToUpper()}) settings window");
            }
            catch (Exception ex)
            {
                LogMessage($"Error opening Primary Camera settings window: {ex.Message}");
                MessageBox.Show($"Failed to open camera settings:\n\n{ex.Message}",
                    "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSecondaryCameraControl_Click(object? sender, EventArgs e)
        {
            try
            {
                var settingsForm = new CameraSettingsForm(serverBaseUrl, secondaryCameraName);
                settingsForm.Show();
                LogMessage($"Opening Secondary Camera ({secondaryCameraName.ToUpper()}) settings window");
            }
            catch (Exception ex)
            {
                LogMessage($"Error opening Secondary Camera settings window: {ex.Message}");
                MessageBox.Show($"Failed to open camera settings:\n\n{ex.Message}",
                    "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCircleSizeIncrease_Click(object? sender, EventArgs e)
        {
            if (circleRadius < MAX_RADIUS)
            {
                circleRadius += 5;
                lblCircleSize.Text = $"{circleRadius}";
                pictureBox2.Invalidate();
            }
        }

        private void BtnCircleSizeDecrease_Click(object? sender, EventArgs e)
        {
            if (circleRadius > MIN_RADIUS)
            {
                circleRadius -= 5;
                lblCircleSize.Text = $"{circleRadius}";
                pictureBox2.Invalidate();
            }
        }

        private void BtnAddCircle_Click(object? sender, EventArgs e)
        {
            isAddingCircle = !isAddingCircle;
            
            if (isAddingCircle)
            {
                btnAddCircle.Text = "Stop Adding";
                btnAddCircle.BackColor = System.Drawing.Color.DarkGreen;
                
                pictureBox2.MouseMove += PictureBox2_MouseMove;
                pictureBox2.MouseClick += PictureBox2_MouseClick;
                pictureBox2.Paint += PictureBox2_Paint;
                pictureBox2.Invalidate();
            }
            else
            {
                btnAddCircle.Text = "Add Circle";
                btnAddCircle.BackColor = System.Drawing.Color.DarkRed;
                
                pictureBox2.MouseMove -= PictureBox2_MouseMove;
                pictureBox2.MouseClick -= PictureBox2_MouseClick;
                pictureBox2.Invalidate();
            }
        }

        private void PictureBox2_MouseMove(object? sender, MouseEventArgs e)
        {
            currentMousePosition = e.Location;
            pictureBox2.Invalidate();
        }

        private void PictureBox2_MouseClick(object? sender, MouseEventArgs e)
        {
            if (isAddingCircle && e.Button == MouseButtons.Left)
            {
                Rectangle displayRect = GetImageDisplayRectangle(pictureBox2);
                
                if (displayRect.Width > 0 && displayRect.Height > 0)
                {
                    int imageX = e.Location.X - displayRect.X;
                    int imageY = e.Location.Y - displayRect.Y;
                    
                    float relativeX = (float)imageX / displayRect.Width;
                    float relativeY = (float)imageY / displayRect.Height;
                    
                    relativeX = Math.Max(0, Math.Min(1, relativeX));
                    relativeY = Math.Max(0, Math.Min(1, relativeY));
                    
                    whiteCirclePositionRelative = new PointF(relativeX, relativeY);
                    whiteCirclePosition = e.Location;
                    whiteCircleRadius = circleRadius;
                    
                    SaveWhiteCirclePosition();
                    pictureBox2.Invalidate();
                    System.Diagnostics.Debug.WriteLine($"White circle placed at: {whiteCirclePosition} ({relativeX:P1}, {relativeY:P1}) in display rect {displayRect} with radius: {whiteCircleRadius}");
                }
            }
        }
    }
}