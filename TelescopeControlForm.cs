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
        private HttpClient videoHttpClient;
        
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
                try
                {
                    var uri = new Uri(serverUrl);
                    this.videoServerUrl = $"{uri.Scheme}://{uri.Host}:5000";
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
                serverClient?.StartStreaming(OnServerDataReceived);
                AddLogMessage("Server streaming started");
                videoStatusTimer = new System.Windows.Forms.Timer();
                videoStatusTimer.Interval = 3000;
                videoStatusTimer.Tick += VideoStatusTimer_Tick;
                videoStatusTimer.Start();
                CheckVideoServerStatus();
            }
            else { grpVideoStream.Visible = false; }
            
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

        private void VideoStatusTimer_Tick(object? sender, EventArgs e) => CheckVideoServerStatus();

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
            if (string.IsNullOrEmpty(videoServerUrl)) { AddLogMessage("Error: Video server URL not available"); return; }
            try
            {
                AddLogMessage("Starting video stream...");
                var response = await videoHttpClient.GetAsync($"{videoServerUrl}/start");
                if (response.IsSuccessStatusCode) { AddLogMessage("Video stream started successfully"); UpdateVideoStatus(true, "Streaming"); }
                else { AddLogMessage($"Failed to start video stream: {await response.Content.ReadAsStringAsync()}"); }
            }
            catch (Exception ex) { AddLogMessage($"Error starting video stream: {ex.Message}"); }
        }

        private async void btnVideoStop_Click(object sender, EventArgs e)
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
            commandTimer?.Stop(); commandTimer?.Dispose();
            focusTimer?.Stop(); focusTimer?.Dispose();
            videoStatusTimer?.Stop(); videoStatusTimer?.Dispose();
            videoHttpClient?.Dispose();
            if (isServerMode && serverClient != null) { serverClient.StopStreaming(); serverClient.Dispose(); }
        }
    }
}
