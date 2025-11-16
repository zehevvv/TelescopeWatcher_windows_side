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
                AddLogMessage("UP arrow key pressed");
            }
            else if (e.KeyCode == Keys.Down)
            {
                isKeyPressed = true;
                currentDirection = "DOWN";
                SendTelescopeCommand("DOWN");
                commandTimer.Start();
                e.Handled = true;
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
