using System.Net.Http;
using System.Text;

namespace TelescopeWatcher
{
    public partial class VideoPlayerForm : Form
    {
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Panel videoPanel;
        private Button btnClose;
        private Label lblStatus;
        private Label lblFrameInfo1;
        private Label lblFrameInfo2;
        private CheckBox chkFlipHorizontal;
        private CheckBox chkFlipVertical;
        private RadioButton radioMainOnly;
        private RadioButton radioSecondaryOnly;
        private RadioButton radioBoth;
        private Panel controlPanel;
        private Button btnAddCircle;
        private Button btnCircleSizeIncrease;
        private Button btnCircleSizeDecrease;
        private Label lblCircleSize;
        private readonly string mjpegUrl1;
        private readonly string mjpegUrl2;
        private HttpClient? httpClient1;
        private HttpClient? httpClient2;
        private CancellationTokenSource? cancellationToken;
        private Task? streamTask1;
        private Task? streamTask2;
        private bool isStreaming = false;
        private int frameCount1 = 0;
        private int frameCount2 = 0;
        private DateTime lastFrameTime1 = DateTime.Now;
        private DateTime lastFrameTime2 = DateTime.Now;
        private DateTime lastFpsUpdate1 = DateTime.Now;
        private DateTime lastFpsUpdate2 = DateTime.Now;
        private bool flipHorizontal = true;
        private bool flipVertical = true;
        
        // Circle overlay fields
        private bool isAddingCircle = false;
        private Point? whiteCirclePosition = null;
        private PointF? whiteCirclePositionRelative = null;  // Store as percentage (0.0 to 1.0)
        private int whiteCircleRadius = 30;
        private int circleRadius = 30;
        private const int MIN_RADIUS = 10;
        private const int MAX_RADIUS = 200;
        private Point currentMousePosition;

        public VideoPlayerForm(string serverUrl)
        {
            this.mjpegUrl1 = $"{serverUrl}:8080/?action=stream";
            this.mjpegUrl2 = $"{serverUrl}:8081/?action=stream";
            InitializeComponent();
            this.FormClosing += VideoPlayerForm_FormClosing;
            LoadWhiteCirclePosition();
        }

        private void InitializeComponent()
        {
            this.Text = "Video Stream - MJPEG";
            this.Size = new System.Drawing.Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            
            // Enable double buffering to reduce flicker
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                          ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint, true);

            // Status label
            lblStatus = new Label
            {
                Text = "Connecting to streams...",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            // Control panel for options
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                Padding = new Padding(10, 5, 10, 5)
            };

            // Stream selection radio buttons
            radioMainOnly = new RadioButton
            {
                Text = "Main Camera",
                Checked = false,
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(10, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            radioMainOnly.CheckedChanged += RadioStream_CheckedChanged;

            radioSecondaryOnly = new RadioButton
            {
                Text = "Secondary Camera",
                Checked = false,
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(130, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            radioSecondaryOnly.CheckedChanged += RadioStream_CheckedChanged;

            radioBoth = new RadioButton
            {
                Text = "Both Cameras",
                Checked = true,
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(280, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            radioBoth.CheckedChanged += RadioStream_CheckedChanged;

            // Horizontal flip checkbox
            chkFlipHorizontal = new CheckBox
            {
                Text = "Flip H",
                Checked = true,
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(420, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            chkFlipHorizontal.CheckedChanged += ChkFlipHorizontal_CheckedChanged;

            // Vertical flip checkbox
            chkFlipVertical = new CheckBox
            {
                Text = "Flip V",
                Checked = true,
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(500, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            chkFlipVertical.CheckedChanged += ChkFlipVertical_CheckedChanged;

            // Circle control buttons
            btnAddCircle = new Button
            {
                Text = "Add Circle",
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.DarkRed,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(580, 5),
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                Padding = new Padding(5, 2, 5, 2)
            };
            btnAddCircle.Click += BtnAddCircle_Click;

            btnCircleSizeDecrease = new Button
            {
                Text = "-",
                Width = 30,
                Height = 28,
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.DarkSlateGray,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(720, 3),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            btnCircleSizeDecrease.Click += BtnCircleSizeDecrease_Click;

            lblCircleSize = new Label
            {
                Text = $"{circleRadius}",
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(755, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };

            btnCircleSizeIncrease = new Button
            {
                Text = "+",
                Width = 30,
                Height = 28,
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.DarkSlateGray,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(810, 3),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            btnCircleSizeIncrease.Click += BtnCircleSizeIncrease_Click;

            controlPanel.Controls.Add(radioMainOnly);
            controlPanel.Controls.Add(radioSecondaryOnly);
            controlPanel.Controls.Add(radioBoth);
            controlPanel.Controls.Add(chkFlipHorizontal);
            controlPanel.Controls.Add(chkFlipVertical);
            controlPanel.Controls.Add(btnAddCircle);
            controlPanel.Controls.Add(btnCircleSizeDecrease);
            controlPanel.Controls.Add(lblCircleSize);
            controlPanel.Controls.Add(btnCircleSizeIncrease);

            // Video panel container
            videoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };

            // Picture box 1 (Main camera)
            pictureBox1 = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.Black,
                Dock = DockStyle.Fill
            };

            // Picture box 2 (Secondary camera)
            pictureBox2 = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.Black,
                Dock = DockStyle.Right,
                Width = 600
            };
            pictureBox2.Paint += PictureBox2_Paint;

            // Frame info labels
            lblFrameInfo1 = new Label
            {
                Text = "Main: Frame 0 | FPS: 0.0",
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LightGray,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Padding = new Padding(10, 0, 0, 0),
                Dock = DockStyle.Bottom
            };

            lblFrameInfo2 = new Label
            {
                Text = "Secondary: Frame 0 | FPS: 0.0",
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LightGray,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Padding = new Padding(10, 0, 0, 0),
                Dock = DockStyle.Bottom
            };

            videoPanel.Controls.Add(pictureBox1);
            videoPanel.Controls.Add(pictureBox2);

            // Close button
            btnClose = new Button
            {
                Text = "Close Stream",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = System.Drawing.Color.IndianRed,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;

            this.Controls.Add(videoPanel);
            this.Controls.Add(controlPanel);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblFrameInfo1);
            this.Controls.Add(lblFrameInfo2);
            this.Controls.Add(btnClose);

            this.Load += VideoPlayerForm_Load;
            this.Resize += VideoPlayerForm_Resize;
        }

        private void VideoPlayerForm_Resize(object? sender, EventArgs e)
        {
            // Recalculate circle position when window is resized
            UpdateWhiteCircleAbsolutePosition();
            pictureBox2.Invalidate();
        }

        private void RadioStream_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioMainOnly.Checked)
            {
                pictureBox1.Visible = true;
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox2.Visible = false;
                pictureBox2.Dock = DockStyle.None;
                lblFrameInfo1.Visible = true;
                lblFrameInfo2.Visible = false;
            }
            else if (radioSecondaryOnly.Checked)
            {
                pictureBox1.Visible = false;
                pictureBox1.Dock = DockStyle.None;
                pictureBox2.Visible = true;
                pictureBox2.Dock = DockStyle.Fill;
                lblFrameInfo1.Visible = false;
                lblFrameInfo2.Visible = true;
            }
            else if (radioBoth.Checked)
            {
                pictureBox1.Visible = true;
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox2.Visible = true;
                pictureBox2.Dock = DockStyle.Right;
                pictureBox2.Width = this.ClientSize.Width / 2;
                lblFrameInfo1.Visible = true;
                lblFrameInfo2.Visible = true;
            }
            
            // Recalculate absolute position from relative when layout changes
            UpdateWhiteCircleAbsolutePosition();
            pictureBox2.Invalidate();
        }

        private async void VideoPlayerForm_Load(object? sender, EventArgs e)
        {
            // Reload circle position after form is fully loaded
            LoadWhiteCirclePosition();
            await StartStreaming();
        }

        private async Task StartStreaming()
        {
            try
            {
                UpdateStatus("Connecting to streams...", System.Drawing.Color.DarkOrange);
                
                httpClient1 = new HttpClient();
                httpClient1.Timeout = TimeSpan.FromMinutes(5);
                httpClient2 = new HttpClient();
                httpClient2.Timeout = TimeSpan.FromMinutes(5);
                
                cancellationToken = new CancellationTokenSource();
                isStreaming = true;

                // Start both streams
                streamTask1 = StartStreamTask(mjpegUrl1, 1);
                streamTask2 = StartStreamTask(mjpegUrl2, 2);

                UpdateStatus("Streams connected - receiving frames...", System.Drawing.Color.DarkGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start video streams:\n\n{ex.Message}", 
                    "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private Task StartStreamTask(string mjpegUrl, int streamId)
        {
            return Task.Run(async () =>
            {
                var httpClient = streamId == 1 ? httpClient1 : httpClient2;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Stream {streamId} - URL: {mjpegUrl}");
                    
                    var request = new HttpRequestMessage(HttpMethod.Get, mjpegUrl);
                    var response = await httpClient!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken!.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"Stream {streamId} - HTTP Error: {response.StatusCode}");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Connected successfully");

                    using var stream = await response.Content.ReadAsStreamAsync();
                    
                    byte[] buffer = new byte[1024];
                    List<byte> frameBuffer = new List<byte>();
                    
                    while (!cancellationToken.Token.IsCancellationRequested)
                    {
                        try
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken.Token);
                            
                            if (bytesRead == 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Ended");
                                break;
                            }

                            for (int i = 0; i < bytesRead; i++)
                            {
                                frameBuffer.Add(buffer[i]);
                                
                                if (frameBuffer.Count < 2)
                                    continue;
                                
                                int len = frameBuffer.Count;
                                
                                if (frameBuffer[len - 2] == 0xFF && frameBuffer[len - 1] == 0xD9)
                                {
                                    int startIndex = -1;
                                    for (int j = 0; j < frameBuffer.Count - 1; j++)
                                    {
                                        if (frameBuffer[j] == 0xFF && frameBuffer[j + 1] == 0xD8)
                                        {
                                            startIndex = j;
                                            break;
                                        }
                                    }
                                    
                                    if (startIndex >= 0)
                                    {
                                        int frameLength = len - startIndex;
                                        byte[] jpegData = new byte[frameLength];
                                        frameBuffer.CopyTo(startIndex, jpegData, 0, frameLength);
                                        
                                        try
                                        {
                                            using var ms = new MemoryStream(jpegData);
                                            var image = Image.FromStream(ms);
                                            
                                            if (flipHorizontal || flipVertical)
                                            {
                                                var bitmap = new Bitmap(image);
                                            
                                                if (flipHorizontal && flipVertical)
                                                {
                                                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                                                }
                                                else if (flipHorizontal)
                                                {
                                                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                                }
                                                else if (flipVertical)
                                                {
                                                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                                }
                                            
                                                UpdateImage(bitmap, streamId);
                                                image.Dispose();
                                            }
                                            else
                                            {
                                                UpdateImage(image, streamId);
                                            }
                                            
                                            if (streamId == 1)
                                            {
                                                frameCount1++;
                                                var now = DateTime.Now;
                                                var elapsed = (now - lastFrameTime1).TotalSeconds;
                                                if (elapsed > 0)
                                                {
                                                    double fps = 1.0 / elapsed;
                                                
                                                    // Only update FPS display every 500ms to reduce flicker
                                                    if ((now - lastFpsUpdate1).TotalMilliseconds >= 500)
                                                    {
                                                        UpdateFrameInfo(frameCount1, fps, 1);
                                                        lastFpsUpdate1 = now;
                                                    }
                                                }
                                                lastFrameTime1 = now;
                                            }
                                            else
                                            {
                                                frameCount2++;
                                                var now = DateTime.Now;
                                                var elapsed = (now - lastFrameTime2).TotalSeconds;
                                                if (elapsed > 0)
                                                {
                                                    double fps = 1.0 / elapsed;
                                                
                                                    // Only update FPS display every 500ms to reduce flicker
                                                    if ((now - lastFpsUpdate2).TotalMilliseconds >= 500)
                                                    {
                                                        UpdateFrameInfo(frameCount2, fps, 2);
                                                        lastFpsUpdate2 = now;
                                                    }
                                                }
                                                lastFrameTime2 = now;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Error decoding: {ex.Message}");
                                        }
                                    }
                                    
                                    frameBuffer.Clear();
                                }
                                
                                if (frameBuffer.Count > 500000)
                                {
                                    frameBuffer.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Read error: {ex.Message}");
                            frameBuffer.Clear();
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Stopped");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Stream {streamId} - Error: {ex}");
                }
            }, cancellationToken!.Token);
        }

        private void UpdateImage(Image image, int streamId)
        {
            var pictureBox = streamId == 1 ? pictureBox1 : pictureBox2;
            
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action<Image, int>(UpdateImage), image, streamId);
                return;
            }

            var oldImage = pictureBox.Image;
            bool wasFirstFrame = (oldImage == null && streamId == 2);
            
            pictureBox.Image = image;
            oldImage?.Dispose();
            
            // If this is the first frame of secondary camera, recalculate circle position
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
            string cameraName = streamId == 1 ? "Main" : "Secondary";
            string newText = $"{cameraName}: Frame {frames} | FPS: {fps:F1}";
            
            if (label.InvokeRequired)
            {
                label.Invoke(new Action<int, double, int>(UpdateFrameInfo), frames, fps, streamId);
                return;
            }

            // Only update if text actually changed to reduce flicker
            if (label.Text != newText)
            {
                label.Text = newText;
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void VideoPlayerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopStreaming();
        }

        private void StopStreaming()
        {
            isStreaming = false;
            cancellationToken?.Cancel();

            try
            {
                streamTask1?.Wait(TimeSpan.FromSeconds(2));
                streamTask2?.Wait(TimeSpan.FromSeconds(2));
            }
            catch { }

            cancellationToken?.Dispose();
            httpClient1?.Dispose();
            httpClient2?.Dispose();
            pictureBox1?.Image?.Dispose();
            pictureBox2?.Image?.Dispose();
        }

        private void ChkFlipHorizontal_CheckedChanged(object? sender, EventArgs e)
        {
            flipHorizontal = chkFlipHorizontal.Checked;
            System.Diagnostics.Debug.WriteLine($"Flip Horizontal: {flipHorizontal}");
        }

        private void ChkFlipVertical_CheckedChanged(object? sender, EventArgs e)
        {
            flipVertical = chkFlipVertical.Checked;
            System.Diagnostics.Debug.WriteLine($"Flip Vertical: {flipVertical}");
        }

        #region Circle Overlay Methods

        private void BtnAddCircle_Click(object? sender, EventArgs e)
        {
            isAddingCircle = !isAddingCircle;
            
            if (isAddingCircle)
            {
                btnAddCircle.Text = "Stop Adding";
                btnAddCircle.BackColor = System.Drawing.Color.DarkGreen;
                
                // Wire up mouse events for pictureBox2
                pictureBox2.MouseMove += PictureBox2_MouseMove;
                pictureBox2.MouseClick += PictureBox2_MouseClick;
                pictureBox2.Paint += PictureBox2_Paint;
                pictureBox2.Invalidate();
            }
            else
            {
                btnAddCircle.Text = "Add Red Circle";
                btnAddCircle.BackColor = System.Drawing.Color.DarkRed;
                
                // Unwire mouse events
                pictureBox2.MouseMove -= PictureBox2_MouseMove;
                pictureBox2.MouseClick -= PictureBox2_MouseClick;
                pictureBox2.Invalidate();
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

        private void PictureBox2_MouseMove(object? sender, MouseEventArgs e)
        {
            currentMousePosition = e.Location;
            pictureBox2.Invalidate();
        }

        private void PictureBox2_MouseClick(object? sender, MouseEventArgs e)
        {
            if (isAddingCircle && e.Button == MouseButtons.Left)
            {
                // Get the actual displayed image rectangle
                Rectangle displayRect = GetImageDisplayRectangle(pictureBox2);
                
                if (displayRect.Width > 0 && displayRect.Height > 0)
                {
                    // Calculate position relative to the actual image area (not the PictureBox)
                    int imageX = e.Location.X - displayRect.X;
                    int imageY = e.Location.Y - displayRect.Y;
                    
                    // Store as percentage of the actual image display area
                    float relativeX = (float)imageX / displayRect.Width;
                    float relativeY = (float)imageY / displayRect.Height;
                    
                    // Clamp to 0-1 range (in case click is in letterbox area)
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

        private void PictureBox2_Paint(object? sender, PaintEventArgs e)
        {
            // Draw white circle (permanent marker) - use saved radius
            if (whiteCirclePosition.HasValue)
            {
                using (Pen whitePen = new Pen(Color.White, 2))
                {
                    int x = whiteCirclePosition.Value.X - whiteCircleRadius;
                    int y = whiteCirclePosition.Value.Y - whiteCircleRadius;
                    e.Graphics.DrawEllipse(whitePen, x, y, whiteCircleRadius * 2, whiteCircleRadius * 2);
                }
            }

            // Draw red circle (cursor preview)
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
                        
                        // Calculate absolute position based on current PictureBox size
                        UpdateWhiteCircleAbsolutePosition();
                        
                        System.Diagnostics.Debug.WriteLine($"Loaded white circle relative position: ({relativeX:P1}, {relativeY:P1}) with radius: {whiteCircleRadius}");
                        
                        // Force repaint to show the loaded circle
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

        private void SaveWhiteCirclePosition()
        {
            try
            {
                if (whiteCirclePositionRelative.HasValue)
                {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "circle_position.txt");
                    File.WriteAllLines(configPath, new[]
                    {
                        whiteCirclePositionRelative.Value.X.ToString("F6"),  // Save as decimal (0.0 to 1.0)
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

        private void UpdateWhiteCircleAbsolutePosition()
        {
            if (whiteCirclePositionRelative.HasValue && pictureBox2 != null && pictureBox2.Image != null)
            {
                // Get the actual displayed image rectangle within the PictureBox (accounts for Zoom mode)
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

        private Rectangle GetImageDisplayRectangle(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
                return Rectangle.Empty;

            // Calculate the actual display rectangle of the image within the PictureBox
            // when SizeMode is Zoom (maintains aspect ratio)
            
            float imageAspect = (float)pictureBox.Image.Width / pictureBox.Image.Height;
            float containerAspect = (float)pictureBox.Width / pictureBox.Height;

            int displayWidth, displayHeight, displayX, displayY;

            if (imageAspect > containerAspect)
            {
                // Image is wider - fit to width, letterbox top/bottom
                displayWidth = pictureBox.Width;
                displayHeight = (int)(pictureBox.Width / imageAspect);
                displayX = 0;
                displayY = (pictureBox.Height - displayHeight) / 2;
            }
            else
            {
                // Image is taller - fit to height, letterbox left/right
                displayHeight = pictureBox.Height;
                displayWidth = (int)(pictureBox.Height * imageAspect);
                displayX = (pictureBox.Width - displayWidth) / 2;
                displayY = 0;
            }

            return new Rectangle(displayX, displayY, displayWidth, displayHeight);
        }

        #endregion
    }
}
