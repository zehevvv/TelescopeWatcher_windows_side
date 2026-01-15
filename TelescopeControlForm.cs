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
        
        private readonly int[] stepsPerSecondValues = { 3, 1, 10, 100, 1000, 10000 };

        public TelescopeControlForm(SerialPort? port, string? serverUrl, string portName)
        {
            InitializeComponent();
            
            videoHttpClient = new HttpClient();
            videoHttpClient.Timeout = TimeSpan.FromSeconds(5);
            
            if (serverUrl != null)
            {
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
                this.serialPort = port;
                this.isServerMode = false;
            }
            
            this.portName = portName;
            lblPortInfo.Text = $"Connected: {portName}";
            this.KeyPreview = true;
            
            commandTimer = new System.Windows.Forms.Timer();
            commandTimer.Interval = 200;
            commandTimer.Tick += CommandTimer_Tick;
            
            focusTimer = new System.Windows.Forms.Timer();
            focusTimer.Interval = 100;
            focusTimer.Tick += FocusTimer_Tick;
            
            if (isServerMode)
            {
                AddLogMessage("Connected to server");
                
                // Start video status polling
                videoStatusTimer = new System.Windows.Forms.Timer();
                videoStatusTimer.Interval = 3000; // 3 seconds
                videoStatusTimer.Tick += VideoStatusTimer_Tick;
                videoStatusTimer.Start();
                
                // Initial status check
                _ = CheckVideoServerStatusAsync();
            }
            else
            {
                grpVideoStream.Visible = false;
            }
            else { grpVideoStream.Visible = false; }
            
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
            this.KeyDown += TelescopeControlForm_KeyDown;
            this.KeyUp += TelescopeControlForm_KeyUp;
            
            UpdateStepsPerSecondDisplay();
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
                            
                            videoPlayerForm = new VideoPlayerForm(baseUrl);
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
                                videoPlayerForm = new VideoPlayerForm(baseUrl);
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
            UpdateStepsPerSecondDisplay();
        }

        private void trackBarFocusSpeed_Scroll(object? sender, EventArgs e)
        {
            UpdateFocusSpeedDisplay();
        }

        private void UpdateFocusSpeedDisplay()
        {
            focusSpeed = trackBarFocusSpeed.Value;
            lblFocusSpeedValue.Text = $"Speed: {focusSpeed}";
            AddLogMessage($"Focus motor speed set to {focusSpeed}");
        }

        private void UpdateStepsPerSecondDisplay()
        {
            int trackBarValue = trackBarStepsPerSecond.Value;
            int stepsPerSecond = stepsPerSecondValues[trackBarValue];
            
            // Calculate time between steps in milliseconds
            double timeMs = 1000.0 / stepsPerSecond;
            timeBetweenSteps = (int)Math.Round(timeMs);
            
            // Ensure minimum time is at least 0.1ms for very high speeds
            if (stepsPerSecond == 10000)
            {
                timeBetweenSteps = 0; // Will send as t=0.1 in the command
            }
            
            // Update display labels
            lblStepsPerSecondValue.Text = $"{stepsPerSecond} steps/second";
            
            if (stepsPerSecond == 10000)
            {
                lblTimeValue.Text = "(t=0.1 ms)";
            }
            else
            {
                lblTimeValue.Text = $"(t={timeMs:F1} ms)";
            }
            
            AddLogMessage($"Speed set to {stepsPerSecond} steps/second (t={(stepsPerSecond == 10000 ? "0.1" : timeMs.ToString("F1"))} ms)");
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

        private async void CheckVideoServerStatus()
        {
            if (string.IsNullOrEmpty(videoServerUrl)) { UpdateVideoStatus(false, "No URL"); return; }
            try
            {
                var response = await videoHttpClient.GetAsync($"{videoServerUrl}/ping");
                UpdateVideoStatus(response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Online" : "Offline");
            }
            catch { UpdateVideoStatus(false, "Offline"); }
        }

        private void UpdateVideoStatus(bool isOnline, string status)
        {
            if (lblVideoStatus.InvokeRequired) { lblVideoStatus.Invoke(new Action(() => UpdateVideoStatus(isOnline, status))); return; }
            lblVideoStatus.Text = status;
            lblVideoStatus.ForeColor = isOnline ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private async void btnVideoStart_Click(object sender, EventArgs e)
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
            if (string.IsNullOrEmpty(videoServerUrl)) { AddLogMessage("Error: Video server URL not available"); return; }
            try
            {
                AddLogMessage("Stopping video stream...");
                var response = await videoHttpClient.GetAsync($"{videoServerUrl}/stop");
                if (response.IsSuccessStatusCode) { AddLogMessage("Video stream stopped successfully"); UpdateVideoStatus(true, "Online"); }
                else { AddLogMessage($"Failed to stop video stream: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { AddLogMessage($"Error stopping video stream: {ex.Message}"); }
        }

        private void trackBarStepsPerSecond_Scroll(object? sender, EventArgs e) => UpdateStepsPerSecondDisplay();
        private void trackBarFocusSpeed_Scroll(object? sender, EventArgs e) => UpdateFocusSpeedDisplay();

        private void UpdateFocusSpeedDisplay()
        {
            focusSpeed = trackBarFocusSpeed.Value;
            lblFocusSpeedValue.Text = $"Speed: {focusSpeed}";
            AddLogMessage($"Focus motor speed set to {focusSpeed}");
        }

        private void UpdateStepsPerSecondDisplay()
        {
            int stepsPerSecond = stepsPerSecondValues[trackBarStepsPerSecond.Value];
            double timeMs = 1000.0 / stepsPerSecond;
            timeBetweenSteps = stepsPerSecond == 10000 ? 0 : (int)Math.Round(timeMs);
            lblStepsPerSecondValue.Text = $"{stepsPerSecond} steps/second";
            lblTimeValue.Text = stepsPerSecond == 10000 ? "(t=0.1 ms)" : $"(t={timeMs:F1} ms)";
            AddLogMessage($"Speed set to {stepsPerSecond} steps/second");
        }

        private void TelescopeControlForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (isKeyPressed || isFocusKeyPressed) { e.Handled = true; e.SuppressKeyPress = true; return; }
            switch (e.KeyCode)
            {
                case Keys.Up: StartDirection("UP"); break;
                case Keys.Down: StartDirection("DOWN"); break;
                case Keys.Left: StartDirection("LEFT"); break;
                case Keys.Right: StartDirection("RIGHT"); break;
                case Keys.PageUp: StartFocus("INCREASE"); break;
                case Keys.PageDown: StartFocus("DECREASE"); break;
                default: return;
            }
            e.Handled = true; e.SuppressKeyPress = true;
        }

        private void StartDirection(string dir) { isKeyPressed = true; currentDirection = dir; SendTelescopeCommand(dir); commandTimer.Start(); AddLogMessage($"{dir} key pressed"); }
        private void StartFocus(string dir) { isFocusKeyPressed = true; currentFocusDirection = dir; SendFocusCommand(dir); focusTimer.Start(); AddLogMessage($"Focus {dir} key pressed"); }

        private void TelescopeControlForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            { isKeyPressed = false; commandTimer.Stop(); SendStopCommand(); currentDirection = ""; e.Handled = true; }
            else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            { isFocusKeyPressed = false; focusTimer.Stop(); SendFocusStopCommand(); currentFocusDirection = ""; e.Handled = true; }
        }

        private void BtnUp_MouseDown(object? sender, MouseEventArgs e) => StartDirection("UP");
        private void BtnUp_MouseUp(object? sender, MouseEventArgs e) { commandTimer.Stop(); SendStopCommand(); currentDirection = ""; }
        private void BtnDown_MouseDown(object? sender, MouseEventArgs e) => StartDirection("DOWN");
        private void BtnDown_MouseUp(object? sender, MouseEventArgs e) { commandTimer.Stop(); SendStopCommand(); currentDirection = ""; }
        private void BtnLeft_MouseDown(object? sender, MouseEventArgs e) => StartDirection("LEFT");
        private void BtnLeft_MouseUp(object? sender, MouseEventArgs e) { commandTimer.Stop(); SendStopCommand(); currentDirection = ""; }
        private void BtnRight_MouseDown(object? sender, MouseEventArgs e) => StartDirection("RIGHT");
        private void BtnRight_MouseUp(object? sender, MouseEventArgs e) { commandTimer.Stop(); SendStopCommand(); currentDirection = ""; }
        private void BtnFocusIncrease_MouseDown(object? sender, MouseEventArgs e) => StartFocus("INCREASE");
        private void BtnFocusIncrease_MouseUp(object? sender, MouseEventArgs e) { focusTimer.Stop(); SendFocusStopCommand(); currentFocusDirection = ""; }
        private void BtnFocusDecrease_MouseDown(object? sender, MouseEventArgs e) => StartFocus("DECREASE");
        private void BtnFocusDecrease_MouseUp(object? sender, MouseEventArgs e) { focusTimer.Stop(); SendFocusStopCommand(); currentFocusDirection = ""; }

        private void FocusTimer_Tick(object? sender, EventArgs e) { if (!string.IsNullOrEmpty(currentFocusDirection)) SendFocusStepsCommand(); }
        private void CommandTimer_Tick(object? sender, EventArgs e) { if (!string.IsNullOrEmpty(currentDirection)) SendStepsCommand(); }

        private void OnServerDataReceived(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;
            try
            {
                string trimmed = data.Trim();
                if (txtLog.InvokeRequired) txtLog.Invoke(new Action(() => AddLogMessage($"Server Response: {trimmed}")));
                else AddLogMessage($"Server Response: {trimmed}");
            }
            catch { }
        }

        private void btnUp_Click(object sender, EventArgs e) { }
        private void btnDown_Click(object sender, EventArgs e) { }
        private void btnLeft_Click(object sender, EventArgs e) { }
        private void btnRight_Click(object sender, EventArgs e) { }
        private void btnFocusIncrease_Click(object sender, EventArgs e) { }
        private void btnFocusDecrease_Click(object sender, EventArgs e) { }

        private void SendFocusCommand(string direction)
        {
            if (!CheckConnection()) return;
            try
            {
                WriteCommand($"b={focusSpeed}"); Thread.Sleep(50);
                WriteCommand(direction == "INCREASE" ? "a=1" : "a=0"); Thread.Sleep(50);
                WriteCommand("c=100");
            }
            catch (Exception ex) { AddLogMessage($"Error: {ex.Message}"); }
        }

        private void SendFocusStepsCommand() { if (CheckConnection()) try { WriteCommand("c=100"); } catch { focusTimer.Stop(); } }
        private void SendFocusStopCommand() { if (CheckConnection()) try { WriteCommand("c=0"); } catch { } }

        private void SendTelescopeCommand(string direction)
        {
            if (!CheckConnection()) return;
            try
            {
                WriteCommand((direction == "UP" || direction == "DOWN") ? "v=0" : "v=1"); Thread.Sleep(50);
                WriteCommand((direction == "UP" || direction == "LEFT") ? "d=0" : "d=1"); Thread.Sleep(50);
                WriteCommand(timeBetweenSteps == 0 ? "t=0.1" : $"t={timeBetweenSteps}"); Thread.Sleep(50);
                WriteCommand("s=10000");
            }
            catch (Exception ex) { AddLogMessage($"Error: {ex.Message}"); }
        }

        private void SendStepsCommand() { if (CheckConnection()) try { WriteCommand("s=10000"); } catch { commandTimer.Stop(); } }
        private void SendStopCommand() { if (CheckConnection()) try { WriteCommand("s=0"); } catch { } }

        private bool CheckConnection()
        {
            if (isServerMode) return serverClient != null && serverClient.IsConnected();
            return serialPort != null && serialPort.IsOpen;
        }

        private void WriteCommand(string command)
        {
            if (isServerMode) serverClient?.WriteLine(command);
            else serialPort?.WriteLine(command);
        }

        private void AddLogMessage(string message)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            if (txtLog.InvokeRequired) txtLog.Invoke(new Action(() => txtLog.AppendText($"[{ts}] {message}\r\n")));
            else txtLog.AppendText($"[{ts}] {message}\r\n");
        }

        private void TelescopeControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
