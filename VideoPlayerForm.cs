using System.Net.Http;
using System.Text;

namespace TelescopeWatcher
{
    public partial class VideoPlayerForm : Form
    {
        private PictureBox pictureBox;
        private Button btnClose;
        private Label lblStatus;
        private Label lblFrameInfo;
        private CheckBox chkFlipHorizontal;
        private CheckBox chkFlipVertical;
        private Panel controlPanel;
        private readonly string mjpegUrl;
        private HttpClient? httpClient;
        private CancellationTokenSource? cancellationToken;
        private Task? streamTask;
        private bool isStreaming = false;
        private int frameCount = 0;
        private DateTime lastFrameTime = DateTime.Now;
        private bool flipHorizontal = true;  // Default: flip horizontal
        private bool flipVertical = true;    // Default: flip vertical

        public VideoPlayerForm(string serverUrl)
        {
            this.mjpegUrl = $"{serverUrl}:8080/?action=stream";
            InitializeComponent();
            this.FormClosing += VideoPlayerForm_FormClosing;
        }

        private void InitializeComponent()
        {
            this.Text = "Video Stream - MJPEG";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(400, 300);

            // Status label
            lblStatus = new Label
            {
                Text = "Connecting to stream...",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };

            // Control panel for flip options
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
                Padding = new Padding(10, 5, 10, 5)
            };

            // Horizontal flip checkbox
            chkFlipHorizontal = new CheckBox
            {
                Text = "Flip Horizontal",
                Checked = true,  // Default: enabled
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(10, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            chkFlipHorizontal.CheckedChanged += ChkFlipHorizontal_CheckedChanged;

            // Vertical flip checkbox
            chkFlipVertical = new CheckBox
            {
                Text = "Flip Vertical",
                Checked = true,  // Default: enabled
                AutoSize = true,
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(150, 8),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            chkFlipVertical.CheckedChanged += ChkFlipVertical_CheckedChanged;

            controlPanel.Controls.Add(chkFlipHorizontal);
            controlPanel.Controls.Add(chkFlipVertical);

            // Frame info label
            lblFrameInfo = new Label
            {
                Text = "Frame: 0 | FPS: 0.0",
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LightGray,
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };

            // Picture box for video
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.Black
            };

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

            this.Controls.Add(pictureBox);
            this.Controls.Add(controlPanel);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblFrameInfo);
            this.Controls.Add(btnClose);

            this.Load += VideoPlayerForm_Load;
        }

        private async void VideoPlayerForm_Load(object? sender, EventArgs e)
        {
            await StartStreaming();
        }

        private async Task StartStreaming()
        {
            try
            {
                UpdateStatus($"Connecting to {mjpegUrl}...", System.Drawing.Color.DarkOrange);
                System.Diagnostics.Debug.WriteLine($"MJPEG URL: {mjpegUrl}");

                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                cancellationToken = new CancellationTokenSource();

                isStreaming = true;

                streamTask = Task.Run(async () =>
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, mjpegUrl);
                        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token);

                        if (!response.IsSuccessStatusCode)
                        {
                            UpdateStatus($"Error: HTTP {response.StatusCode}", System.Drawing.Color.DarkRed);
                            System.Diagnostics.Debug.WriteLine($"HTTP Error: {response.StatusCode}");
                            return;
                        }

                        UpdateStatus("Stream connected - receiving frames...", System.Drawing.Color.DarkGreen);
                        System.Diagnostics.Debug.WriteLine("Stream connected successfully");
                        System.Diagnostics.Debug.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");

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
                                    UpdateStatus("Stream ended", System.Drawing.Color.DarkRed);
                                    break;
                                }

                                // Add bytes to frame buffer
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    frameBuffer.Add(buffer[i]);
                                    
                                    // Check if we have at least 2 bytes
                                    if (frameBuffer.Count < 2)
                                        continue;
                                    
                                    int len = frameBuffer.Count;
                                    
                                    // Look for JPEG end marker (FF D9)
                                    if (frameBuffer[len - 2] == 0xFF && frameBuffer[len - 1] == 0xD9)
                                    {
                                        // Found complete JPEG frame
                                        // Now find the start marker (FF D8)
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
                                            // Extract complete JPEG (from start to end marker)
                                            int frameLength = len - startIndex;
                                            byte[] jpegData = new byte[frameLength];
                                            frameBuffer.CopyTo(startIndex, jpegData, 0, frameLength);
                                            
                                            try
                                            {
                                                using var ms = new MemoryStream(jpegData);
                                                var image = Image.FromStream(ms);
                                                
                                                // Apply flip transformations based on user settings
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
                                                    
                                                    UpdateImage(bitmap);
                                                    image.Dispose();
                                                }
                                                else
                                                {
                                                    UpdateImage(image);
                                                }
                                                
                                                frameCount++;
                                                var now = DateTime.Now;
                                                var elapsed = (now - lastFrameTime).TotalSeconds;
                                                if (elapsed > 0)
                                                {
                                                    double fps = 1.0 / elapsed;
                                                    UpdateFrameInfo(frameCount, fps);
                                                }
                                                lastFrameTime = now;
                                                
                                                System.Diagnostics.Debug.WriteLine($"Frame {frameCount} decoded successfully ({jpegData.Length} bytes)");
                                            }
                                            catch (Exception ex)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Error decoding frame: {ex.Message} (data length: {jpegData.Length})");
                                            }
                                        }
                                        
                                        // Clear buffer after processing frame
                                        frameBuffer.Clear();
                                    }
                                    
                                    // Prevent buffer from growing too large (keep last 500KB)
                                    if (frameBuffer.Count > 500000)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Buffer overflow - clearing");
                                        frameBuffer.Clear();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Frame read error: {ex.Message}");
                                frameBuffer.Clear();
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        UpdateStatus("Stream stopped", System.Drawing.Color.Gray);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Error: {ex.Message}", System.Drawing.Color.DarkRed);
                        System.Diagnostics.Debug.WriteLine($"Stream error: {ex}");
                    }
                }, cancellationToken.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start video stream:\n\n{ex.Message}\n\nURL: {mjpegUrl}", 
                    "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void UpdateImage(Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action<Image>(UpdateImage), image);
                return;
            }

            var oldImage = pictureBox.Image;
            pictureBox.Image = image;
            oldImage?.Dispose();
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

        private void UpdateFrameInfo(int frames, double fps)
        {
            if (lblFrameInfo.InvokeRequired)
            {
                lblFrameInfo.Invoke(new Action<int, double>(UpdateFrameInfo), frames, fps);
                return;
            }

            lblFrameInfo.Text = $"Frame: {frames} | FPS: {fps:F1}";
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
                streamTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch { }

            cancellationToken?.Dispose();
            httpClient?.Dispose();
            pictureBox?.Image?.Dispose();
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
    }
}
