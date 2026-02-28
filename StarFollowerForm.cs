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
        private System.Windows.Forms.Timer statusTimer;

        public StarFollowerForm(string serverUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverUrl;
            cbCamera.SelectedIndex = 0; // Default to 'hd'

            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            this.FormClosing += StarFollowerForm_FormClosing;
        }

        private void StarFollowerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
        }

        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            await CheckStatusAsync();
        }

        private async Task CheckStatusAsync()
        {
            string url = $"{serverBaseUrl}:5000/star_follower/status";
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("active", out JsonElement activeElement))
                    {
                        bool isActive = activeElement.GetBoolean();
                        UpdateStatusLabel(isActive);
                    }
                }
                else
                {
                    UpdateStatusLabel(false, "Server Error");
                }
            }
            catch
            {
                UpdateStatusLabel(false, "Connection Error");
            }
        }

        private void UpdateStatusLabel(bool isActive, string overrideText = null)
        {
            if (lblActiveStatus.InvokeRequired)
            {
                lblActiveStatus.Invoke(new Action(() => UpdateStatusLabel(isActive, overrideText)));
                return;
            }

            if (overrideText != null)
            {
                lblActiveStatus.Text = $"Status: {overrideText}";
                lblActiveStatus.ForeColor = System.Drawing.Color.DarkOrange;
            }
            else
            {
                lblActiveStatus.Text = isActive ? "Status: Active" : "Status: Inactive";
                lblActiveStatus.ForeColor = isActive ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
            }
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
