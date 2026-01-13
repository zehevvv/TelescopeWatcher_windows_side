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
        private bool flipHorizontal = true;
        private bool flipVertical = true;

        public VideoPlayerForm(string serverUrl)
        {
            this.mjpegUrl1 = $"{serverUrl}:8080/?action=stream";
            this.mjpegUrl2 = $"{serverUrl}:8081/?action=stream";
            InitializeComponent();
            this.FormClosing += VideoPlayerForm_FormClosing;
        }

        private void InitializeComponent()
        {
            this.Text = "Video Stream - MJPEG";
            this.Size = new System.Drawing.Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(600, 400);

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

            controlPanel.Controls.Add(radioMainOnly);
            controlPanel.Controls.Add(radioSecondaryOnly);
            controlPanel.Controls.Add(radioBoth);
            controlPanel.Controls.Add(chkFlipHorizontal);
            controlPanel.Controls.Add(chkFlipVertical);

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
        }

        private void RadioStream_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioMainOnly.Checked)
            {
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox2.Visible = false;
                lblFrameInfo2.Visible = false;
            }
            else if (radioSecondaryOnly.Checked)
            {
                pictureBox1.Visible = false;
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
        }

        private async void VideoPlayerForm_Load(object? sender, EventArgs e)
        {
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
                                                    UpdateFrameInfo(frameCount1, fps, 1);
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
                                                    UpdateFrameInfo(frameCount2, fps, 2);
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

        private void UpdateFrameInfo(int frames, double fps, int streamId)
        {
            var label = streamId == 1 ? lblFrameInfo1 : lblFrameInfo2;
            string cameraName = streamId == 1 ? "Main" : "Secondary";
            
            if (label.InvokeRequired)
            {
                label.Invoke(new Action<int, double, int>(UpdateFrameInfo), frames, fps, streamId);
                return;
            }

            label.Text = $"{cameraName}: Frame {frames} | FPS: {fps:F1}";
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
    }
}
