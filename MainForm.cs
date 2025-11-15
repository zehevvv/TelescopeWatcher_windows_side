using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class MainForm : Form
    {
        private SerialPort? serialPort;
        private string? selectedPort;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPortList();
            AddStatusMessage("Application started. Please select a COM port and click Connect.");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshPortList();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (listBoxPorts.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port first.", "No Port Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            selectedPort = listBoxPorts.SelectedItem.ToString();
            ConnectToPort(selectedPort!);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectFromPort();
        }

        private void RefreshPortList()
        {
            listBoxPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                AddStatusMessage("No COM ports found.");
                listBoxPorts.Items.Add("No ports available");
                return;
            }

            foreach (string port in ports)
            {
                listBoxPorts.Items.Add(port);
            }

            AddStatusMessage($"Found {ports.Length} COM port(s): {string.Join(", ", ports)}");
            
            if (listBoxPorts.Items.Count > 0)
            {
                listBoxPorts.SelectedIndex = 0;
            }
        }

        private void ConnectToPort(string portName)
        {
            try
            {
                serialPort = new SerialPort(portName)
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();

                AddStatusMessage($"Successfully connected to {portName}");
                AddStatusMessage($"Settings: {serialPort.BaudRate} baud, {serialPort.DataBits} data bits, {serialPort.Parity} parity, {serialPort.StopBits} stop bits");

                // Update UI
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnRefresh.Enabled = false;
                listBoxPorts.Enabled = false;
            }
            catch (Exception ex)
            {
                AddStatusMessage($"Error connecting to {portName}: {ex.Message}");
                MessageBox.Show($"Failed to connect to {portName}.\r\n\r\nError: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectFromPort()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    AddStatusMessage($"Disconnected from {selectedPort}");
                }
                catch (Exception ex)
                {
                    AddStatusMessage($"Error disconnecting: {ex.Message}");
                }
                finally
                {
                    serialPort.Dispose();
                    serialPort = null;
                }
            }

            // Update UI
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnRefresh.Enabled = true;
            listBoxPorts.Enabled = true;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string data = serialPort.ReadExisting();
                    // Use Invoke to safely update UI from another thread
                    this.Invoke(new Action(() =>
                    {
                        AddStatusMessage($"Received: {data}");
                    }));
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    AddStatusMessage($"Error reading data: {ex.Message}");
                }));
            }
        }

        private void AddStatusMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtStatus.AppendText($"[{timestamp}] {message}\r\n");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectFromPort();
        }
    }
}
