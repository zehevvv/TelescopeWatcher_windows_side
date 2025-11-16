using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class TelescopeControlForm : Form
    {
        private SerialPort serialPort;
        private string portName;

        public TelescopeControlForm(SerialPort port, string portName)
        {
            InitializeComponent();
            this.serialPort = port;
            this.portName = portName;
            lblPortInfo.Text = $"Connected: {portName} @ {port.BaudRate} baud";
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            SendTelescopeCommand("UP");
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            SendTelescopeCommand("DOWN");
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

                // Send direction command
                serialPort.WriteLine(directionCommand);
                Thread.Sleep(50); // Small delay between commands

                // Send steps command
                string stepsCommand = "s=100";
                serialPort.WriteLine(stepsCommand);
                AddLogMessage("Sending: s=100 (Steps: 100)");
                AddLogMessage($"Command sent successfully: Move {direction} 100 steps");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error sending command: {ex.Message}");
                MessageBox.Show($"Failed to send command.\r\n\r\nError: {ex.Message}", 
                    "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Don't close the serial port here - let the main form handle it
            AddLogMessage("Telescope control window closed.");
        }
    }
}
