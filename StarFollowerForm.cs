using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class StarFollowerForm : Form
    {
        private readonly string serverBaseUrl;
        private static readonly HttpClient httpClient = new HttpClient() 
        { 
            Timeout = TimeSpan.FromSeconds(30)
        };

        public StarFollowerForm(string serverUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverUrl;
            cbCamera.SelectedIndex = 0; // Default to 'hd'
        }

        private void AppendOutput(string text)
        {
            if (txtOutput.InvokeRequired)
            {
                txtOutput.Invoke(new Action(() => AppendOutput(text)));
                return;
            }
            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}\r\n");
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            string camera = cbCamera.SelectedItem?.ToString() ?? "hd";
            float duration = (float)numDuration.Value;
            float threshold = (float)numThreshold.Value;
            string stepsCmd = Uri.EscapeDataString(txtStepsCmd.Text);
            string speedCmd = Uri.EscapeDataString(txtSpeedCmd.Text);

            string url = $"{serverBaseUrl}:5000/star_follower/start?camera={camera}&duration={duration}&threshold={threshold}&steps_cmd={stepsCmd}&speed_cmd={speedCmd}";

            AppendOutput("Starting tracking...");
            await SendGetRequest(url);
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            string url = $"{serverBaseUrl}:5000/star_follower/stop";
            AppendOutput("Stopping tracking...");
            await SendGetRequest(url);
        }

        private async void btnStatus_Click(object sender, EventArgs e)
        {
            string url = $"{serverBaseUrl}:5000/star_follower/status";
            AppendOutput("Getting status...");
            await SendGetRequest(url);
        }

        private async void btnDebug_Click(object sender, EventArgs e)
        {
            string camera = cbCamera.SelectedItem?.ToString() ?? "hd";
            string url = $"{serverBaseUrl}:5000/star_follower/debug_star?camera={camera}";
            AppendOutput($"Getting debug info for {camera}...");
            await SendGetRequest(url);
        }

        private async Task SendGetRequest(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
                        string formattedJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });
                        AppendOutput($"Success:\r\n{formattedJson}");
                    }
                    catch
                    {
                        AppendOutput($"Success: {json}");
                    }
                }
                else
                {
                    AppendOutput($"Error {response.StatusCode}: {json}");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed: {ex.Message}");
            }
        }
    }
}
