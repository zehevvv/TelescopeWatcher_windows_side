using System.IO.Ports;

namespace TelescopeWatcher
{
    public partial class MainForm : Form
    {
        private SerialPort? serialPort;
        private string? selectedPort;
        private TelescopeControlForm? telescopeControlForm;
        private bool isServerMode = true; // Default to server mode
        private string serverUrl = "http://192.168.4.1:5002";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshPortList();
            UpdateUIForConnectionMode();
            AddStatusMessage("Application started. Default mode: HTTP Server (192.168.4.1:5002)");
        }

        private void radioSerial_CheckedChanged(object sender, EventArgs e)
        {
            if (radioSerial.Checked)
            {
                isServerMode = false;
                UpdateUIForConnectionMode();
                AddStatusMessage("Connection mode: USB Serial");
            }
        }

        private void radioServer_CheckedChanged(object sender, EventArgs e)
        {
            if (radioServer.Checked)
            {
                isServerMode = true;
                UpdateUIForConnectionMode();
                AddStatusMessage("Connection mode: HTTP Server");
            }
        }

        private void UpdateUIForConnectionMode()
        {
            if (isServerMode)
            {
                txtServerUrl.Enabled = true;
                listBoxPorts.Enabled = false;
                btnRefresh.Enabled = false;
                lblPorts.Text = "Server Mode - Port 5002";
            }
            else
            {
                txtServerUrl.Enabled = false;
                listBoxPorts.Enabled = true;
                btnRefresh.Enabled = true;
                lblPorts.Text = "Available COM Ports:";
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshPortList();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (isServerMode)
            {
                ConnectToServer();
            }
            else
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
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectFromPort();
        }

        private void ConnectToServer()
        {
            try
            {
                string serverIp = txtServerUrl.Text.Trim();
                
                if (string.IsNullOrEmpty(serverIp))
                {
                    MessageBox.Show("Please enter a server IP address.", "No Server IP", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Construct full URL with fixed port 5002 and add http:// if missing
                if (!serverIp.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !serverIp.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    serverUrl = $"http://{serverIp}:5002";
                }
                else
                {
                    // If user included http://, just add port
                    serverUrl = serverIp.Contains(":") ? serverIp : $"{serverIp}:5002";
                }
                
                AddStatusMessage($"Connecting to server: {serverUrl}");
                
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync($"{serverUrl}/read").Result;
                    
                    if (response.IsSuccessStatusCode)
                    {
                        AddStatusMessage($"Successfully connected to server: {serverUrl}");
                        
                        btnConnect.Enabled = false;
                        btnDisconnect.Enabled = true;
                        radioSerial.Enabled = false;
                        radioServer.Enabled = false;
                        txtServerUrl.Enabled = false;
                        
                        OpenTelescopeControlForServer();
                    }
                    else
                    {
                        throw new Exception($"Server returned status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddStatusMessage($"Error connecting to server: {ex.Message}");
                MessageBox.Show($"Failed to connect to server.\r\n\r\nError: {ex.Message}\r\n\r\nMake sure the server is running on {serverUrl}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    BaudRate = 115200,
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

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnRefresh.Enabled = false;
                listBoxPorts.Enabled = false;
                radioSerial.Enabled = false;
                radioServer.Enabled = false;

                OpenTelescopeControl();
            }
            catch (Exception ex)
            {
                AddStatusMessage($"Error connecting to {portName}: {ex.Message}");
                MessageBox.Show($"Failed to connect to {portName}.\r\n\r\nError: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenTelescopeControl()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                telescopeControlForm = new TelescopeControlForm(serialPort, null, selectedPort!);
                telescopeControlForm.Show();
                AddStatusMessage("Telescope control window opened.");
            }
        }

        private void OpenTelescopeControlForServer()
        {
            telescopeControlForm = new TelescopeControlForm(null, serverUrl, $"Server: {serverUrl}");
            telescopeControlForm.Show();
            AddStatusMessage("Telescope control window opened.");
        }

        private void DisconnectFromPort()
        {
            if (telescopeControlForm != null && !telescopeControlForm.IsDisposed)
            {
                telescopeControlForm.Close();
                telescopeControlForm = null;
            }

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
            
            if (isServerMode)
            {
                AddStatusMessage($"Disconnected from server: {serverUrl}");
            }

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            radioSerial.Enabled = true;
            radioServer.Enabled = true;
            UpdateUIForConnectionMode();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string data = serialPort.ReadExisting();
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
