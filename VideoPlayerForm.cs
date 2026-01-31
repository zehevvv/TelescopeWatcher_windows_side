using System.Net.Http;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace TelescopeWatcher
{
    public partial class VideoPlayerForm : Form
    {
        private readonly string serverBaseUrl;
        private readonly string mjpegUrl2;
        private HttpClient? httpClient2;
        private CancellationTokenSource? cancellationToken;
        private Task? streamTask2;
        private bool isStreaming = false;
        private int frameCount2 = 0;
        private DateTime lastFrameTime2 = DateTime.Now;
        private DateTime lastFpsUpdate2 = DateTime.Now;
        private bool flipHorizontal = true;
        private bool flipVertical = true;

        private WebView2? webView;
        
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
        private MjpegStreamClient mjpegClient;
        
        // Removed separate connection fields managed by controller
        private bool isKeyPressed = false;
        private bool isFocusKeyPressed = false;
        private string currentDirection = "";
        private string currentFocusDirection = "";
        private System.Windows.Forms.Timer commandTimer;
        private System.Windows.Forms.Timer focusTimer;
        private System.Windows.Forms.Timer fpsTimer; // Added frame timer
        private int lastDisplayedPictures = 0; // For FPS calc

        private Action<string>? logCallback;

        public VideoPlayerForm(string serverUrl, SerialPort? port = null, SerialServerClient? client = null, 
                               int stepsPerSecond = 100, int focusMotorSpeed = 9, Action<string>? logCallback = null)
        {
            this.serverBaseUrl = serverUrl;
            
            try
            {
                var uri = new Uri(serverUrl);
                this.mjpegUrl2 = $"{uri.Scheme}://{uri.Host}:5002/?action=stream";
            }
            catch
            {
                // Fallback for raw IP or other formats
                this.mjpegUrl2 = $"{serverUrl}:5002/?action=stream"; 
            }
            
            this.logCallback = logCallback;
            
            // Initialize Helpers
            this.telescopeController = new TelescopeController(port, client, logCallback);
            this.mjpegClient = new MjpegStreamClient();
            this.mjpegClient.FrameReceived += MjpegClient_FrameReceived;
            
            // Use shared settings via Controller
            var settings = TelescopeSettings.Instance;
            telescopeController.TimeBetweenSteps = settings.TimeBetweenSteps;
            telescopeController.FocusSpeed = settings.FocusSpeed;
            
            // Subscribe to settings changes
            settings.StepsPerSecondChanged += OnStepsPerSecondChanged;
            settings.FocusSpeedChanged += OnFocusSpeedChanged;
            
            // Initialize other fields (retaining defaults in case)
            settings.StepsPerSecond = stepsPerSecond;
            settings.FocusSpeed = focusMotorSpeed;

            InitializeComponent();
            
            // Initialize WebView2
            this.webView = new WebView2();
            this.webView.Name = "webViewMain";
            this.webView.Dock = DockStyle.Fill;
            this.Controls.Add(this.webView);
            
            // Hide the old VideoView if it exists
            if (this.Controls.ContainsKey("videoView1"))
            {
                this.Controls["videoView1"].Visible = false;
            }

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
            double timeMs = stepsPerSecond == 10000 ? 0.1 : 1000.0 / stepsPerSecond;
            lblStepsPerSecondValue.Text = $"{stepsPerSecond} steps/sec (t={timeMs:F1}ms)";
        }

        private void UpdateFocusSpeedDisplay()
        {
            var settings = TelescopeSettings.Instance;
            lblFocusSpeedValue.Text = $"Speed: {settings.FocusSpeed}";
        }

        private void PositionCircleControls()
        {
            if (controlPanel == null) return;
            
            int rightMargin = 10;
            int yPos = 3;
            
            // Position from right to left: + button, label, - button, Add Circle button
            btnCircleSizeIncrease.Location = new System.Drawing.Point(controlPanel.Width - rightMargin - btnCircleSizeIncrease.Width, yPos);
            
            lblCircleSize.Location = new System.Drawing.Point(btnCircleSizeIncrease.Left - 5 - lblCircleSize.Width, yPos + 5);
            
            btnCircleSizeDecrease.Location = new System.Drawing.Point(lblCircleSize.Left - 5 - btnCircleSizeDecrease.Width, yPos);
            
            btnAddCircle.Location = new System.Drawing.Point(btnCircleSizeDecrease.Left - 10 - btnAddCircle.Width, yPos + 2);
        }

        private void PositionFocusControls()
        {
            if (telescopeControlPanel == null) return;
            
            int rightMargin = 10;
            int labelYPos = 10;
            int trackbarYPos = 30;
            int valueYPos = 35;
            
            // Position focus speed value label from the right
            lblFocusSpeedValue.Location = new System.Drawing.Point(
                telescopeControlPanel.Width - rightMargin - lblFocusSpeedValue.PreferredWidth, 
                valueYPos
            );
            
            // Position trackbar to the left of the value label
            trackBarFocusSpeed.Location = new System.Drawing.Point(
                lblFocusSpeedValue.Left - 10 - trackBarFocusSpeed.Width, 
                trackbarYPos
            );
            
            // Position the label above the trackbar
            lblFocusSpeed.Location = new System.Drawing.Point(
                trackBarFocusSpeed.Left, 
                labelYPos
            );
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
            UpdateWhiteCircleAbsolutePosition();
            pictureBox2.Invalidate();
        }

        private void RadioStream_CheckedChanged(object? sender, EventArgs e)
        {
            if (webView == null) return;

            if (radioMainOnly.Checked)
            {
                webView.Visible = true;
                webView.Dock = DockStyle.Fill;
                webView.BringToFront();
                pictureBox2.Visible = false;
                pictureBox2.Dock = DockStyle.None;
                lblFrameInfo1.Visible = true;
                lblFrameInfo2.Visible = false;
            }
            else if (radioSecondaryOnly.Checked)
            {
                webView.Visible = false;
                webView.Dock = DockStyle.None;
                pictureBox2.Visible = true;
                pictureBox2.Dock = DockStyle.Fill;
                lblFrameInfo1.Visible = false;
                lblFrameInfo2.Visible = true;
            }
            else if (radioBoth.Checked)
            {
                webView.Visible = true;
                webView.Dock = DockStyle.Fill;
                pictureBox2.Visible = true;
                pictureBox2.Dock = DockStyle.Right;
                // Note: WebView2 might fight for layout space if not careful.
                // Dock Right for pictureBox2 gets priority, Fill fills remaining.
                pictureBox2.Width = this.ClientSize.Width / 2;
                
                // Ensure pictureBox2 is docked first (Right) by sending it to back of Z-order
                pictureBox2.SendToBack();
                webView.BringToFront();

                lblFrameInfo1.Visible = true;
                lblFrameInfo2.Visible = true;
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
                
                // Initialize WebView2 for Main Stream
                try 
                {
                    if (webView != null)
                    {
                        await webView.EnsureCoreWebView2Async();
                        
                        var host = new Uri(serverBaseUrl).Host;
                        var webUrl = $"http://{host}:8889/cam";
                        
                        System.Diagnostics.Debug.WriteLine($"Connecting to Main Web Stream: {webUrl}");
                        
                        webView.Source = new Uri(webUrl);
                        UpdateFrameInfo(0, 0, 1); // Reset text (FPS not available for web)
                    }
                }
                catch (Exception ex)
                {
                     System.Diagnostics.Debug.WriteLine($"Failed to start WebView2: {ex.Message}");
                     MessageBox.Show($"Failed to initialize Web player: {ex.Message}. Ensure WebView2 Runtime is installed.", "Web Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Initialize MJPEG for Secondary Stream
                mjpegClient.FlipHorizontal = flipHorizontal;
                mjpegClient.FlipVertical = flipVertical;
                await mjpegClient.StartStream(mjpegUrl2, 2);

                UpdateStatus("Streams connected", System.Drawing.Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start video streams:\n\n{ex.Message}", 
                    "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void MjpegClient_FrameReceived(object? sender, Image image)
        {
            UpdateImage(image, 2);
            
            // FPS calculation logic for Mjpeg could be moved inside client too, but kept here for now
            frameCount2++;
            var now = DateTime.Now;
            var elapsed = (now - lastFrameTime2).TotalSeconds;
            if (elapsed > 0)
            {
                double fps = 1.0 / elapsed;
                if ((now - lastFpsUpdate2).TotalMilliseconds >= 500)
                {
                    UpdateFrameInfo(frameCount2, fps, 2);
                    lastFpsUpdate2 = now;
                }
            }
            lastFrameTime2 = now;
        }

        private void UpdateImage(Image image, int streamId)
        {
            // Only for Stream 2 (Secondary)
            if (streamId != 2) return;
            
            if (pictureBox2.InvokeRequired)
            {
                pictureBox2.Invoke(new Action<Image, int>(UpdateImage), image, streamId);
                return;
            }

            var oldImage = pictureBox2.Image;
            bool wasFirstFrame = (oldImage == null);
            
            pictureBox2.Image = image;
            oldImage?.Dispose();
            
            if (wasFirstFrame && whiteCirclePositionRelative.HasValue)
            {
                UpdateWhiteCircleAbsolutePosition();
                pictureBox2.Invalidate();
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
                newText = $"Main: WebRTC Stream"; // Removed FPS
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
            // FPS logic for main stream is not available with WebView2
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
            
            mjpegClient?.Dispose();
            webView?.Dispose();
        }

        private void StopStreaming()
        {
            isStreaming = false;
            mjpegClient?.StopStreaming();

            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.Source = new Uri("about:blank");
            }

            pictureBox2?.Image?.Dispose();
        }

        private void ChkFlipHorizontal_CheckedChanged(object? sender, EventArgs e)
        {
            flipHorizontal = chkFlipHorizontal.Checked;
            if (mjpegClient != null) mjpegClient.FlipHorizontal = flipHorizontal;
            System.Diagnostics.Debug.WriteLine($"Flip Horizontal: {flipHorizontal}");
        }

        private void ChkFlipVertical_CheckedChanged(object? sender, EventArgs e)
        {
            flipVertical = chkFlipVertical.Checked;
            if (mjpegClient != null) mjpegClient.FlipVertical = flipVertical;
            System.Diagnostics.Debug.WriteLine($"Flip Vertical: {flipVertical}");
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
                string url = "http://192.168.4.1:8080/control.htm";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                LogMessage($"Opening Main Camera control page: {url}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error opening Main Camera control page: {ex.Message}");
                MessageBox.Show($"Failed to open camera control page:\n\n{ex.Message}",
                    "Browser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSecondaryCameraControl_Click(object? sender, EventArgs e)
        {
            try
            {
                // Update to port 5002 for UC60 (MJPG) control
                var host = new Uri(serverBaseUrl).Host;
                string url = $"http://{host}:5002/control.htm";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                LogMessage($"Opening Secondary Camera control page: {url}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error opening Secondary Camera control page: {ex.Message}");
                MessageBox.Show($"Failed to open camera control page:\n\n{ex.Message}",
                    "Browser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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