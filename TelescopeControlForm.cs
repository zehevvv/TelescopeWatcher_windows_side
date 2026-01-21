using System.Diagnostics;
using System.IO.Ports;
using System.Net.Http;

namespace TelescopeWatcher
{
    public partial class TelescopeControlForm : Form
    {
        private SerialPort? serialPort;
        private SerialServerClient? serverClient;
        private string portName;
        private System.Windows.Forms.Timer commandTimer;
        private System.Windows.Forms.Timer focusTimer;
        private System.Windows.Forms.Timer? videoStatusTimer;
        private string currentDirection = "";
        private string currentFocusDirection = "";
        private bool isKeyPressed = false;
        private bool isFocusKeyPressed = false;
        private int timeBetweenSteps = 10;
        private int focusSpeed = 9;
        private bool isServerMode = false;
        private string? videoServerUrl;
        private HttpClient? videoHttpClient;
        private VideoPlayerForm? videoPlayerForm;

        // Steps per second values corresponding to trackbar positions
        private readonly int[] stepsPerSecondValues = { 3, 10, 100, 1000, 10000 };

        public TelescopeControlForm(SerialPort? port, string? serverUrl, string portName)
        {
            InitializeComponent();

            if (serverUrl != null)
            {
                // Server mode
                this.serverClient = new SerialServerClient(serverUrl);
                this.isServerMode = true;

                // Extract video server URL (port 5000)
                try
                {
                    var uri = new Uri(serverUrl);
                    this.videoServerUrl = $"{uri.Scheme}://{uri.Host}:5000";

                    // Initialize video HTTP client with short timeout
                    this.videoHttpClient = new HttpClient();
                    this.videoHttpClient.Timeout = TimeSpan.FromSeconds(2);
                }
                catch { this.videoServerUrl = null; }
            }
            else
            {
                // Serial mode
                this.serialPort = port;
                this.isServerMode = false;
            }

            this.portName = portName;
            lblPortInfo.Text = $"Connected: {portName}";

            // Enable key preview to capture keyboard events
            this.KeyPreview = true;

            // Initialize timer for continuous commands
            commandTimer = new System.Windows.Forms.Timer();
            commandTimer.Interval = 200;
            commandTimer.Tick += CommandTimer_Tick;

            // Initialize focus timer for continuous focus commands
            focusTimer = new System.Windows.Forms.Timer();
            focusTimer.Interval = 100;
            focusTimer.Tick += FocusTimer_Tick;
            
            // Initialize shared settings
            var settings = TelescopeSettings.Instance;
            this.focusSpeed = settings.FocusSpeed;
            this.timeBetweenSteps = settings.TimeBetweenSteps;
            
            // Subscribe to settings changes
            settings.StepsPerSecondChanged += OnStepsPerSecondChanged;
            settings.FocusSpeedChanged += OnFocusSpeedChanged;

            if (isServerMode)
            {
                AddLogMessage("Connected to server");

                // Start video status polling
                videoStatusTimer = new System.Windows.Forms.Timer();
                videoStatusTimer.Interval = 3000;
                videoStatusTimer.Tick += VideoStatusTimer_Tick;
                videoStatusTimer.Start();

                // Initial status check
                _ = CheckVideoServerStatusAsync();
            }
            else
            {
                grpVideoStream.Visible = false;
            }

            // Wire up button events
            btnUp.MouseDown += BtnUp_MouseDown;
            btnUp.MouseUp += BtnUp_MouseUp;
            btnDown.MouseDown += BtnDown_MouseDown;
            btnDown.MouseUp += BtnDown_MouseUp;
            btnLeft.MouseDown += BtnLeft_MouseDown;
            btnLeft.MouseUp += BtnLeft_MouseUp;
            btnRight.MouseDown += BtnRight_MouseDown;
            btnRight.MouseUp += BtnRight_MouseUp;
            btnFocusIncrease.MouseDown += BtnFocusIncrease_MouseDown;
            btnFocusIncrease.MouseUp += BtnFocusIncrease_MouseUp;
            btnFocusDecrease.MouseDown += BtnFocusDecrease_MouseDown;
            btnFocusDecrease.MouseUp += BtnFocusDecrease_MouseUp;

            // Wire up keyboard events
            this.KeyDown += TelescopeControlForm_KeyDown;
            this.KeyUp += TelescopeControlForm_KeyUp;

            // Set default trackbar value and update display
            UpdateStepsPerSecondDisplay();
            UpdateFocusSpeedDisplay();
        }

        private void OnStepsPerSecondChanged(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnStepsPerSecondChanged(sender, e)));
                return;
            }
            
            var settings = TelescopeSettings.Instance;
            this.timeBetweenSteps = settings.TimeBetweenSteps;
            
            // Update trackbar without triggering event
            trackBarStepsPerSecond.Scroll -= trackBarStepsPerSecond_Scroll;
            trackBarStepsPerSecond.Value = settings.GetTrackbarIndexForStepsPerSecond();
            trackBarStepsPerSecond.Scroll += trackBarStepsPerSecond_Scroll;
            
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
            this.focusSpeed = settings.FocusSpeed;
            
            // Update trackbar without triggering event
            trackBarFocusSpeed.Scroll -= trackBarFocusSpeed_Scroll;
            trackBarFocusSpeed.Value = settings.FocusSpeed;
            trackBarFocusSpeed.Scroll += trackBarFocusSpeed_Scroll;
            
            UpdateFocusSpeedDisplay();
        }

        #region Video Stream Control

        private void VideoStatusTimer_Tick(object? sender, EventArgs e)
        {
            _ = CheckVideoServerStatusAsync();
        }

        private async Task CheckVideoServerStatusAsync()
        {
            if (string.IsNullOrEmpty(videoServerUrl) || videoHttpClient == null)
            {
                UpdateVideoStatus(false, "No URL");
                return;
            }

            try
            {
                var response = await videoHttpClient.GetAsync($"{videoServerUrl}/ping");
                UpdateVideoStatus(response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Online" : "Offline");
            }
            catch
            {
                UpdateVideoStatus(false, "Offline");
            }
        }

        private void UpdateVideoStatus(bool isOnline, string status)
        {
            if (lblVideoStatus.InvokeRequired)
            {
                lblVideoStatus.Invoke(new Action(() => UpdateVideoStatus(isOnline, status)));
                return;
            }

            lblVideoStatus.Text = status;
            lblVideoStatus.ForeColor = isOnline ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private async void btnVideoStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(videoServerUrl) || videoHttpClient == null)
            {
                AddLogMessage("Error: Video server URL not available");
                return;
            }

            try
            {
                btnVideoStart.Enabled = false;
                AddLogMessage("Starting video streams...");

                // Extract base URL for secondary server
                var uri = new Uri(videoServerUrl);
                string secondaryServerUrl = $"{uri.Scheme}://{uri.Host}:5001";

                // Start both video servers
                var response1Task = videoHttpClient.GetAsync($"{videoServerUrl}/start");
                var response2Task = videoHttpClient.GetAsync($"{secondaryServerUrl}/start");

                var response1 = await response1Task;
                var response2 = await response2Task;

                bool mainSuccess = response1.IsSuccessStatusCode;
                bool secondarySuccess = response2.IsSuccessStatusCode;

                if (mainSuccess && secondarySuccess)
                {
                    AddLogMessage("Both video streams started successfully");
                    UpdateVideoStatus(true, "Streaming");

                    // Open video player window
                    if (videoPlayerForm == null || videoPlayerForm.IsDisposed)
                    {
                        try
                        {
                            string baseUrl = $"{uri.Scheme}://{uri.Host}";

                            // Get current steps per second from shared settings
                            var settings = TelescopeSettings.Instance;
                            int stepsPerSecond = settings.StepsPerSecond;

                            // Pass telescope control dependencies to video player
                            videoPlayerForm = new VideoPlayerForm(
                                baseUrl, 
                                serialPort, 
                                serverClient, 
                                stepsPerSecond, 
                                settings.FocusSpeed,
                                AddLogMessage
                            );
                            videoPlayerForm.Show();
                            videoPlayerForm.FormClosed += (s, args) =>
                            {
                                videoPlayerForm = null;
                                AddLogMessage("Video player window closed");
                            };

                            AddLogMessage($"Video player opened - Main: {baseUrl}:8080, Secondary: {baseUrl}:8081");
                        }
                        catch (Exception ex)
                        {
                            AddLogMessage($"Error opening video player: {ex.Message}");
                            MessageBox.Show($"Failed to open video player:\n\n{ex.Message}",
                                "Video Player Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        videoPlayerForm.BringToFront();
                        AddLogMessage("Video player already open");
                    }
                }
                else
                {
                    string error = "";
                    if (!mainSuccess)
                    {
                        string mainError = await response1.Content.ReadAsStringAsync();
                        error += $"Main stream (port 5000): {mainError}\n";
                        AddLogMessage($"Failed to start main video stream: {mainError}");
                    }
                    else
                    {
                        AddLogMessage("Main video stream started successfully");
                    }

                    if (!secondarySuccess)
                    {
                        string secondaryError = await response2.Content.ReadAsStringAsync();
                        error += $"Secondary stream (port 5001): {secondaryError}";
                        AddLogMessage($"Failed to start secondary video stream: {secondaryError}");
                    }
                    else
                    {
                        AddLogMessage("Secondary video stream started successfully");
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show($"Failed to start one or more video streams:\n\n{error}",
                            "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // Open player even if only one stream started
                    if (mainSuccess || secondarySuccess)
                    {
                        if (videoPlayerForm == null || videoPlayerForm.IsDisposed)
                        {
                            try
                            {
                                string baseUrl = $"{uri.Scheme}://{uri.Host}";
                                
                                // Get current steps per second from shared settings
                                var settings = TelescopeSettings.Instance;
                                int stepsPerSecond = settings.StepsPerSecond;
                                
                                // Pass telescope control dependencies to video player
                                videoPlayerForm = new VideoPlayerForm(
                                    baseUrl, 
                                    serialPort, 
                                    serverClient, 
                                    stepsPerSecond, 
                                    settings.FocusSpeed,
                                    AddLogMessage
                                );
                                videoPlayerForm.Show();
                                videoPlayerForm.FormClosed += (s, args) =>
                                {
                                    videoPlayerForm = null;
                                    AddLogMessage("Video player window closed");
                                };
                            }
                            catch (Exception ex)
                            {
                                AddLogMessage($"Error opening video player: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                AddLogMessage("Video start request timed out");
                UpdateVideoStatus(false, "Timeout");
                MessageBox.Show("Video start request timed out.\n\nThe server may be busy or unreachable.",
                    "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error starting video streams: {ex.Message}");
                MessageBox.Show($"Error starting video streams:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnVideoStart.Enabled = true;
            }
        }

        private async void btnVideoStop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(videoServerUrl) || videoHttpClient == null)
            {
                AddLogMessage("Error: Video server URL not available");
                return;
            }

            try
            {
                btnVideoStop.Enabled = false;
                AddLogMessage("Stopping video streams...");

                // Close video player window if open
                if (videoPlayerForm != null && !videoPlayerForm.IsDisposed)
                {
                    videoPlayerForm.Close();
                    videoPlayerForm = null;
                    AddLogMessage("Video player window closed");
                }

                // Extract base URL for secondary server
                var uri = new Uri(videoServerUrl);
                string secondaryServerUrl = $"{uri.Scheme}://{uri.Host}:5001";

                // Stop both video servers
                var response1Task = videoHttpClient.GetAsync($"{videoServerUrl}/stop");
                var response2Task = videoHttpClient.GetAsync($"{secondaryServerUrl}/stop");

                var response1 = await response1Task;
                var response2 = await response2Task;

                bool mainSuccess = response1.IsSuccessStatusCode;
                bool secondarySuccess = response2.IsSuccessStatusCode;

                if (mainSuccess && secondarySuccess)
                {
                    AddLogMessage("Both video streams stopped successfully");
                    UpdateVideoStatus(true, "Online");
                }
                else
                {
                    if (!mainSuccess)
                    {
                        string error = await response1.Content.ReadAsStringAsync();
                        AddLogMessage($"Failed to stop main video stream: {error}");
                    }
                    else
                    {
                        AddLogMessage("Main video stream stopped successfully");
                    }

                    if (!secondarySuccess)
                    {
                        string error = await response2.Content.ReadAsStringAsync();
                        AddLogMessage($"Failed to stop secondary video stream: {error}");
                    }
                    else
                    {
                        AddLogMessage("Secondary video stream stopped successfully");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                AddLogMessage("Video stop request timed out");
                UpdateVideoStatus(false, "Timeout");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error stopping video streams: {ex.Message}");
            }
            finally
            {
                btnVideoStop.Enabled = true;
            }
        }

        private async void btnRestart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(videoServerUrl) || videoHttpClient == null)
            {
                AddLogMessage("Error: Video server URL not available");
                return;
            }

            try
            {
                btnRestart.Enabled = false;
                AddLogMessage("Sending restart command to server...");

                // Extract base URL for restart endpoint
                var uri = new Uri(videoServerUrl);
                string restartUrl = $"{uri.Scheme}://{uri.Host}:5000/restart";

                // Send restart command
                var response = await videoHttpClient.GetAsync(restartUrl);

                if (response.IsSuccessStatusCode)
                {
                    AddLogMessage("Restart command sent successfully");
                    MessageBox.Show("Restart command sent to server.\n\nThe server will restart shortly.",
                        "Restart", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    AddLogMessage($"Failed to send restart command: {error}");
                    MessageBox.Show($"Failed to send restart command.\n\n{error}",
                        "Restart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TaskCanceledException)
            {
                AddLogMessage("Restart request timed out");
                MessageBox.Show("Restart request timed out.\n\nThe server may be busy or unreachable.",
                    "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending restart command: {ex.Message}");
                MessageBox.Show($"Error sending restart command:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRestart.Enabled = true;
            }
        }

        #endregion

        #region Server Data Streaming

        private void OnServerDataReceived(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            try
            {
                string trimmed = data.Trim();
                if (txtLog.InvokeRequired)
                {
                    txtLog.Invoke(new Action(() => AddLogMessage($"Server: {trimmed}")));
                }
                else
                {
                    AddLogMessage($"Server: {trimmed}");
                }
            }
            catch { }
        }

        #endregion

        private void trackBarStepsPerSecond_Scroll(object? sender, EventArgs e)
        {
            var settings = TelescopeSettings.Instance;
            settings.SetStepsPerSecondFromTrackbarIndex(trackBarStepsPerSecond.Value);
            UpdateStepsPerSecondDisplay();
        }

        private void trackBarFocusSpeed_Scroll(object? sender, EventArgs e)
        {
            var settings = TelescopeSettings.Instance;
            settings.FocusSpeed = trackBarFocusSpeed.Value;
            UpdateFocusSpeedDisplay();
        }

        private void UpdateFocusSpeedDisplay()
        {
            var settings = TelescopeSettings.Instance;
            lblFocusSpeedValue.Text = $"Speed: {settings.FocusSpeed}";
            AddLogMessage($"Focus motor speed set to {settings.FocusSpeed}");
        }

        private void UpdateStepsPerSecondDisplay()
        {
            var settings = TelescopeSettings.Instance;
            int stepsPerSecond = settings.StepsPerSecond;

            // Update display labels
            lblStepsPerSecondValue.Text = $"{stepsPerSecond} steps/second";

            if (stepsPerSecond == 10000)
            {
                lblTimeValue.Text = "(t=0.1 ms)";
            }
            else
            {
                double timeMs = 1000.0 / stepsPerSecond;
                lblTimeValue.Text = $"(t={timeMs:F1} ms)";
            }

            AddLogMessage($"Speed set to {stepsPerSecond} steps/second (t={(stepsPerSecond == 10000 ? "0.1" : (1000.0 / stepsPerSecond).ToString("F1"))} ms)");
        }

        private void TelescopeControlForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Prevent auto-repeat of KeyDown events
            if (isKeyPressed || isFocusKeyPressed)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true; // Prevent default arrow key behavior
                }
                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                isKeyPressed = true;
                currentDirection = "UP";
                SendTelescopeCommand("UP");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent default arrow key behavior
                AddLogMessage("UP arrow key pressed");
            }
            else if (e.KeyCode == Keys.Down)
            {
                isKeyPressed = true;
                currentDirection = "DOWN";
                SendTelescopeCommand("DOWN");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent default arrow key behavior
                AddLogMessage("DOWN arrow key pressed");
            }
            else if (e.KeyCode == Keys.Left)
            {
                isKeyPressed = true;
                currentDirection = "LEFT";
                SendTelescopeCommand("LEFT");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent default arrow key behavior
                AddLogMessage("LEFT arrow key pressed");
            }
            else if (e.KeyCode == Keys.Right)
            {
                isKeyPressed = true;
                currentDirection = "RIGHT";
                SendTelescopeCommand("RIGHT");
                commandTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent default arrow key behavior
                AddLogMessage("RIGHT arrow key pressed");
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                isFocusKeyPressed = true;
                currentFocusDirection = "INCREASE";
                SendFocusCommand("INCREASE");
                focusTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                AddLogMessage("PageUp key pressed - Focus increase");
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                isFocusKeyPressed = true;
                currentFocusDirection = "DECREASE";
                SendFocusCommand("DECREASE");
                focusTimer.Start();
                e.Handled = true;
                e.SuppressKeyPress = true;
                AddLogMessage("PageDown key pressed - Focus decrease");
            }
        }

        private void TelescopeControlForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                isKeyPressed = false;
                commandTimer.Stop();
                SendStopCommand();
                string keyName = e.KeyCode == Keys.Up ? "UP" :
                                 e.KeyCode == Keys.Down ? "DOWN" :
                                 e.KeyCode == Keys.Left ? "LEFT" : "RIGHT";
                AddLogMessage($"{keyName} arrow key released - stopped sending commands");
                currentDirection = "";
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                isFocusKeyPressed = false;
                focusTimer.Stop();
                SendFocusStopCommand();
                string keyName = e.KeyCode == Keys.PageUp ? "PageUp" : "PageDown";
                AddLogMessage($"{keyName} key released - stopped focus commands");
                currentFocusDirection = "";
                e.Handled = true;
            }
        }

        private void BtnUp_MouseDown(object? sender, MouseEventArgs e)
        {
            currentDirection = "UP";
            SendTelescopeCommand("UP");
            commandTimer.Start();
        }

        private void BtnUp_MouseUp(object? sender, MouseEventArgs e)
        {
            commandTimer.Stop();
            SendStopCommand();
            currentDirection = "";
            AddLogMessage("UP button released - stopped sending commands");
        }

        private void BtnDown_MouseDown(object? sender, MouseEventArgs e)
        {
            currentDirection = "DOWN";
            SendTelescopeCommand("DOWN");
            commandTimer.Start();
        }

        private void BtnDown_MouseUp(object? sender, MouseEventArgs e)
        {
            commandTimer.Stop();
            SendStopCommand();
            currentDirection = "";
            AddLogMessage("DOWN button released - stopped sending commands");
        }

        private void BtnLeft_MouseDown(object? sender, MouseEventArgs e)
        {
            currentDirection = "LEFT";
            SendTelescopeCommand("LEFT");
            commandTimer.Start();
        }

        private void BtnLeft_MouseUp(object? sender, MouseEventArgs e)
        {
            commandTimer.Stop();
            SendStopCommand();
            currentDirection = "";
            AddLogMessage("LEFT button released - stopped sending commands");
        }

        private void BtnRight_MouseDown(object? sender, MouseEventArgs e)
        {
            currentDirection = "RIGHT";
            SendTelescopeCommand("RIGHT");
            commandTimer.Start();
        }

        private void BtnRight_MouseUp(object? sender, MouseEventArgs e)
        {
            commandTimer.Stop();
            SendStopCommand();
            currentDirection = "";
            AddLogMessage("RIGHT button released - stopped sending commands");
        }

        private void BtnFocusIncrease_MouseDown(object? sender, MouseEventArgs e)
        {
            currentFocusDirection = "INCREASE";
            SendFocusCommand("INCREASE");
            focusTimer.Start();
        }

        private void BtnFocusIncrease_MouseUp(object? sender, MouseEventArgs e)
        {
            focusTimer.Stop();
            SendFocusStopCommand();
            currentFocusDirection = "";
            AddLogMessage("Focus INCREASE button released");
        }

        private void BtnFocusDecrease_MouseDown(object? sender, MouseEventArgs e)
        {
            currentFocusDirection = "DECREASE";
            SendFocusCommand("DECREASE");
            focusTimer.Start();
        }

        private void BtnFocusDecrease_MouseUp(object? sender, MouseEventArgs e)
        {
            focusTimer.Stop();
            SendFocusStopCommand();
            currentFocusDirection = "";
            AddLogMessage("Focus DECREASE button released");
        }

        private void FocusTimer_Tick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFocusDirection))
            {
                SendFocusStepsCommand();
            }
        }

        private void CommandTimer_Tick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentDirection))
            {
                SendStepsCommand();
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void btnFocusIncrease_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void btnFocusDecrease_Click(object sender, EventArgs e)
        {
            // Keep for compatibility but MouseDown/MouseUp will handle continuous commands
        }

        private void SendFocusCommand(string direction)
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    AddLogMessage("Error: Server connection is not available!");
                    MessageBox.Show("Server is not connected.", "Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    AddLogMessage("Error: Serial port is not open!");
                    MessageBox.Show("Serial port is not connected.", "Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                // Set focus motor speed (b=xx where xx is 1-18)
                string speedCommand = $"b={focusSpeed}";
                WriteCommand(speedCommand);
                AddLogMessage($"Sending: b={focusSpeed} (Focus Speed: {focusSpeed})");
                Thread.Sleep(50);

                // Set focus direction (a=x where x is 0 or 1)
                string directionCommand;
                if (direction == "INCREASE")
                {
                    directionCommand = "a=1";
                    AddLogMessage("Sending: a=1 (Focus Direction: INCREASE/FAR)");
                }
                else // DECREASE
                {
                    directionCommand = "a=0";
                    AddLogMessage("Sending: a=0 (Focus Direction: DECREASE/NEAR)");
                }
                WriteCommand(directionCommand);
                Thread.Sleep(50);

                // Send focus steps command (c=100 for 100 steps)
                string stepsCommand = "c=100";
                WriteCommand(stepsCommand);
                AddLogMessage("Sending: c=100 (Focus Steps: 100)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending focus command: {ex.Message}");
                MessageBox.Show($"Failed to send focus command.\r\n\r\nError: {ex.Message}",
                    "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendFocusStepsCommand()
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    focusTimer.Stop();
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    focusTimer.Stop();
                    return;
                }
            }

            try
            {
                // Send only focus steps command (direction and speed already set)
                string stepsCommand = "c=100";
                WriteCommand(stepsCommand);
                AddLogMessage("Sending: c=100 (Focus Steps: 100)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending focus steps command: {ex.Message}");
                focusTimer.Stop();
            }
        }

        private void SendFocusStopCommand()
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    return;
                }
            }

            try
            {
                // Send focus stop command (c=0 for 0 steps)
                string stopCommand = "c=0";
                WriteCommand(stopCommand);
                AddLogMessage("Sending: c=0 (Focus STOP)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending focus stop command: {ex.Message}");
            }
        }

        private void SendTelescopeCommand(string direction)
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    AddLogMessage("Error: Server connection is not available!");
                    MessageBox.Show("Server is not connected.", "Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    AddLogMessage("Error: Serial port is not open!");
                    MessageBox.Show("Serial port is not connected.", "Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                string motorCommand;
                string directionCommand;

                // Determine motor selection and direction based on command
                if (direction == "UP")
                {
                    motorCommand = "v=0"; // Up/Down motor
                    directionCommand = "d=0";
                    AddLogMessage("Sending: v=0 (Motor: UP/DOWN)");
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                    AddLogMessage("Sending: d=1 (Direction: UP)");
                }
                else if (direction == "DOWN")
                {
                    motorCommand = "v=0"; // Up/Down motor
                    directionCommand = "d=1";
                    AddLogMessage("Sending: v=0 (Motor: UP/DOWN)");
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                    AddLogMessage("Sending: d=0 (Direction: DOWN)");
                }
                else if (direction == "LEFT")
                {
                    motorCommand = "v=1"; // Left/Right motor
                    directionCommand = "d=0";
                    AddLogMessage("Sending: v=1 (Motor: LEFT/RIGHT)");
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                    AddLogMessage("Sending: d=0 (Direction: LEFT)");
                }
                else // RIGHT
                {
                    motorCommand = "v=1"; // Left/Right motor
                    directionCommand = "d=1";
                    AddLogMessage("Sending: v=1 (Motor: LEFT/RIGHT)");
                    WriteCommand(motorCommand);
                    Thread.Sleep(50);
                    AddLogMessage("Sending: d=1 (Direction: RIGHT)");
                }

                // Send direction command
                WriteCommand(directionCommand);
                Thread.Sleep(50); // Small delay between commands

                // Send time between steps command
                string timeCommand;
                string timeDisplay;

                // Handle special case for 10000 steps/second (t=0.1)
                if (timeBetweenSteps == 0)
                {
                    timeCommand = "t=0.1";
                    timeDisplay = "0.1";
                }
                else
                {
                    timeCommand = $"t={timeBetweenSteps}";
                    timeDisplay = timeBetweenSteps.ToString();
                }

                WriteCommand(timeCommand);
                AddLogMessage($"Sending: {timeCommand} (Time: {timeDisplay}ms)");
                Thread.Sleep(50); // Small delay between commands

                // Send steps command
                string stepsCommand = "s=10000";
                WriteCommand(stepsCommand);
                AddLogMessage("Sending: s=10000 (Steps: 10000)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending command: {ex.Message}");
                MessageBox.Show($"Failed to send command.\r\n\r\nError: {ex.Message}",
                    "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendStepsCommand()
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    commandTimer.Stop();
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    commandTimer.Stop();
                    return;
                }
            }

            try
            {
                // Send only steps command (direction already set)
                string stepsCommand = "s=10000";
                WriteCommand(stepsCommand);
                AddLogMessage("Sending: s=10000 (Steps: 10000)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending command: {ex.Message}");
                commandTimer.Stop();
            }
        }

        private void SendStopCommand()
        {
            if (isServerMode)
            {
                if (serverClient == null || !serverClient.IsConnected())
                {
                    return;
                }
            }
            else
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    return;
                }
            }

            try
            {
                // Send stop command
                string stopCommand = "s=0";
                WriteCommand(stopCommand);
                AddLogMessage("Sending: s=0 (STOP)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending stop command: {ex.Message}");
            }
        }

        private void WriteCommand(string command)
        {
            if (isServerMode)
            {
                serverClient?.WriteLine(command);
            }
            else
            {
                serialPort?.WriteLine(command);
            }
        }

        private void AddLogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() =>
                {
                    txtLog.AppendText($"[{timestamp}] {message}\r\n");
                }));
            }
            else
            {
                txtLog.AppendText($"[{timestamp}] {message}\r\n");
            }
        }

        private void TelescopeControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from static events to prevent memory leaks and disposed access errors
            var settings = TelescopeSettings.Instance;
            settings.StepsPerSecondChanged -= OnStepsPerSecondChanged;
            settings.FocusSpeedChanged -= OnFocusSpeedChanged;


            commandTimer?.Stop();
            commandTimer?.Dispose();
            focusTimer?.Stop();
            focusTimer?.Dispose();

            videoStatusTimer?.Stop();
            videoStatusTimer?.Dispose();
            videoHttpClient?.Dispose();

            // Close video player window if open
            if (videoPlayerForm != null && !videoPlayerForm.IsDisposed)
            {
                videoPlayerForm.Close();
                videoPlayerForm = null;
            }

            serverClient?.Dispose();
        }
    }
}
