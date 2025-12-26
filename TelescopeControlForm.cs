using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class TelescopeControlForm : Form
    {
        private SerialPort? serialPort;
        private SerialServerClient? serverClient;
        private string portName;
        private System.Windows.Forms.Timer commandTimer;
        private System.Windows.Forms.Timer focusTimer;
        private string currentDirection = "";
        private string currentFocusDirection = "";
        private bool isKeyPressed = false;
        private bool isFocusKeyPressed = false;
        private int timeBetweenSteps = 10; // Default 10ms (100 steps/second)
        private int focusSpeed = 9; // Default focus motor speed (1-18)
        private bool isServerMode = false;
        
        // Steps per second values corresponding to trackbar positions
        private readonly int[] stepsPerSecondValues = { 3, 1, 10, 100, 1000, 10000 };

        public TelescopeControlForm(SerialPort? port, string? serverUrl, string portName)
        {
            InitializeComponent();
            
            if (serverUrl != null)
            {
                // Server mode
                this.serverClient = new SerialServerClient(serverUrl);
                this.isServerMode = true;
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
            commandTimer.Interval = 200; // 200ms interval
            commandTimer.Tick += CommandTimer_Tick;
            
            // Initialize focus timer for continuous focus commands
            focusTimer = new System.Windows.Forms.Timer();
            focusTimer.Interval = 100; // 100ms interval
            focusTimer.Tick += FocusTimer_Tick;
            
            // Wire up MouseDown and MouseUp events for buttons
            btnUp.MouseDown += BtnUp_MouseDown;
            btnUp.MouseUp += BtnUp_MouseUp;
            btnDown.MouseDown += BtnDown_MouseDown;
            btnDown.MouseUp += BtnDown_MouseUp;
            btnLeft.MouseDown += BtnLeft_MouseDown;
            btnLeft.MouseUp += BtnLeft_MouseUp;
            btnRight.MouseDown += BtnRight_MouseDown;
            btnRight.MouseUp += BtnRight_MouseUp;
            
            // Wire up focus button events
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
            // Stop timers and cleanup
            commandTimer?.Stop();
            commandTimer?.Dispose();
            focusTimer?.Stop();
            focusTimer?.Dispose();
            
            // Cleanup server client if in server mode
            if (isServerMode && serverClient != null)
            {
                serverClient.Dispose();
            }
            
            // Don't close the serial port here - let the main form handle it
            AddLogMessage("Telescope control window closed.");
        }
    }
}
