using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class CalibrationForm : Form
    {
        private readonly string serverBaseUrl;
        private readonly HttpClient httpClient;

        public CalibrationForm(string serverBaseUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverBaseUrl;
            
            // Setup HttpClient with long timeout
            this.httpClient = new HttpClient();
            this.httpClient.Timeout = TimeSpan.FromSeconds(30); // > 10s as requested
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string camera = txtCamera.Text.Trim();
            string cmd = txtCmd.Text;

            // Handle literal "\n" input as newline if intended, or just keep raw.
            // Requirement said "cmd can include note like /n that should send as one note"
            // Assuming user types "\n" and wants a newline char
            cmd = cmd.Replace("\\n", "\n");

            lblResult.Text = "Sending command...";
            btnSend.Enabled = false;

            try
            {
                // UriBuilder or manual construction
                var builder = new UriBuilder(serverBaseUrl);
                // Ensure base URL has port if needed: serverBaseUrl usually has scheme+host+port
                // The prompt says "server side i added new endpoint /cam/check_rotation"
                // Assuming serverBaseUrl is like "http://192.168.1.10:5000"
                
                // Construct URL manually to avoid UriBuilder complexity with existing base string
                string baseUrl = serverBaseUrl.TrimEnd('/');
                // Ensure we use the correct port if the user provided one, or the dedicated controller port?
                // CameraSettingsForm uses: $"{uri.Scheme}://{uri.Host}:5000"
                // VideoPlayerForm passes `serverUrl`. We should check how `serverUrl` is passed.
                // Assuming `serverBaseUrl` passed to constructor includes port or we derive it.
                // CameraSettingsForm logic:
                // var uri = new Uri(serverBaseUrl); this.apiUrl = $"{uri.Scheme}://{uri.Host}:5000";
                
                string apiUrl;
                try
                {
                    var uri = new Uri(serverBaseUrl);
                    apiUrl = $"{uri.Scheme}://{uri.Host}:5000";
                }
                catch
                {
                    apiUrl = $"{serverBaseUrl}:5000";
                }

                string url = $"{apiUrl}/cam/check_rotation?camera={Uri.EscapeDataString(camera)}&cmd={Uri.EscapeDataString(cmd)}";
                
                System.Diagnostics.Debug.WriteLine($"Sending calibration request: {url}");
                
                var response = await httpClient.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    lblResult.Text = $"Result: {responseBody}";
                    lblResult.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblResult.Text = $"Error: {response.StatusCode} - {responseBody}";
                    lblResult.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = $"Exception: {ex.Message}";
                lblResult.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }
    }
}
