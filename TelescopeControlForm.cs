using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class TelescopeControlForm : Form
    {
        private SerialPort serialPort;
        private string portName;
        private System.Windows.Forms.Timer commandTimer;
        private string currentDirection = "";
        private bool isKeyPressed = false;
        private int timeBetweenSteps = 5000; // Default 5000ms

        public TelescopeControlForm(SerialPort port, string portName)
        {
            InitializeComponent();
            this.serialPort = port;
            this.portName = portName;
            lblPortInfo.Text = $"Connected: {portName} @ {port.BaudRate} baud";
            
            // Enable key preview to capture keyboard events
            this.KeyPreview = true;
            
            // Initialize timer for continuous commands
            commandTimer = new System.Windows.Forms.Timer();
            commandTimer.Interval = 200; // 200ms interval
            commandTimer.Tick += CommandTimer_Tick;
            
            // Wire up MouseDown and MouseUp events for buttons
            btnUp.MouseDown += BtnUp_MouseDown;
            btnUp.MouseUp += BtnUp_MouseUp;
            btnDown.MouseDown += BtnDown_MouseDown;
            btnDown.MouseUp += BtnDown_MouseUp;
            
            // Wire up keyboard events
            this.KeyDown += TelescopeControlForm_KeyDown;
            this.KeyUp += TelescopeControlForm_KeyUp;
            
            // Set default time value
            txtCustomTime.Text = timeBetweenSteps.ToString();
        }

        private void radio5000_CheckedChanged(object? sender, EventArgs e)
        {
            if (radio5000.Checked)
            {
                timeBetweenSteps = 5000;
                txtCustomTime.Text = "5000";
                AddLogMessage("Time between steps set to 5000ms");
            }
        }

        private void radio10000_CheckedChanged(object? sender, EventArgs e)
        {
            if (radio10000.Checked)
            {
                timeBetweenSteps = 10000;
                txtCustomTime.Text = "10000";
                AddLogMessage("Time between steps set to 10000ms");
            }
        }

        private void radio15000_CheckedChanged(object? sender, EventArgs e)
        {
            if (radio15000.Checked)
            {
                timeBetweenSteps = 15000;
                txtCustomTime.Text = "15000";
                AddLogMessage("Time between steps set to 15000ms");
            }
        }

        private void txtCustomTime_TextChanged(object? sender, EventArgs e)
        {
            if (int.TryParse(txtCustomTime.Text, out int customTime))
            {
                if (customTime > 0)
                {
                    timeBetweenSteps = customTime;
                    // Uncheck all radio buttons when custom value is entered
                    radio5000.Checked = false;
                    radio10000.Checked = false;
                    radio15000.Checked = false;
                }
            }
        }

        private void TelescopeControlForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Prevent auto-repeat of KeyDown events
            if (isKeyPressed)
                return;

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
        }

        private void TelescopeControlForm_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                isKeyPressed = false;
                commandTimer.Stop();
                SendStopCommand();
                AddLogMessage($"{(e.KeyCode == Keys.Up ? "UP" : "DOWN")} arrow key released - stopped sending commands");
                currentDirection = "";
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

        private void SendTelescopeCommand(string direction)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                AddLogMessage("Error: Serial port is not open!");
                MessageBox.Show("Serial port is not connected.", "Connection Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string directionCommand;
                if (direction == "UP")
                {
                    directionCommand = "d=1";
                    AddLogMessage("Sending: d=1 (Direction: UP)");
                }
                else // DOWN
                {
                    directionCommand = "d=0";
                    AddLogMessage("Sending: d=0 (Direction: DOWN)");
                }

                // Send direction command (only once when button first pressed)
                serialPort.WriteLine(directionCommand);
                Thread.Sleep(50); // Small delay between commands

                // Send time between steps command
                string timeCommand = $"t={timeBetweenSteps}";
                serialPort.WriteLine(timeCommand);
                AddLogMessage($"Sending: t={timeBetweenSteps} (Time: {timeBetweenSteps}ms)");
                Thread.Sleep(50); // Small delay between commands

                // Send steps command
                string stepsCommand = "s=100";
                serialPort.WriteLine(stepsCommand);
                AddLogMessage("Sending: s=100 (Steps: 100)");
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
            if (serialPort == null || !serialPort.IsOpen)
            {
                commandTimer.Stop();
                return;
            }

            try
            {
                // Send only steps command (direction already set)
                string stepsCommand = "s=100";
                serialPort.WriteLine(stepsCommand);
                AddLogMessage("Sending: s=100 (Steps: 100)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending command: {ex.Message}");
                commandTimer.Stop();
            }
        }

        private void SendStopCommand()
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }

            try
            {
                // Send stop command
                string stopCommand = "s=0";
                serialPort.WriteLine(stopCommand);
                AddLogMessage("Sending: s=0 (STOP)");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending stop command: {ex.Message}");
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
            // Stop timer and cleanup
            commandTimer?.Stop();
            commandTimer?.Dispose();
            
            // Don't close the serial port here - let the main form handle it
            AddLogMessage("Telescope control window closed.");
        }
    }
}
