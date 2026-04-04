using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class SiderealTrackerForm : Form
    {
        private readonly string serverBaseUrl;
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        private System.Windows.Forms.Timer statusTimer;

        public SiderealTrackerForm(string serverUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverUrl;

            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 2000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            this.FormClosing += SiderealTrackerForm_FormClosing;
        }

        private void SiderealTrackerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
        }

        private async void StatusTimer_Tick(object? sender, EventArgs e)
        {
            await RefreshStatusAsync(silent: true);
        }

        private async Task RefreshStatusAsync(bool silent = false)
        {
            string url = $"{serverBaseUrl}:5000/sidereal/status";
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);
                    bool isActive = doc.RootElement.TryGetProperty("active", out var activeEl) && activeEl.GetBoolean();
                    UpdateStatusLabel(isActive);

                    if (!silent)
                    {
                        var formatted = JsonSerializer.Serialize(
                            JsonSerializer.Deserialize<JsonElement>(json),
                            new JsonSerializerOptions { WriteIndented = true });
                        AppendOutput($"Status:\r\n{formatted}");
                    }
                }
                else
                {
                    UpdateStatusLabel(false, "Server Error");
                    if (!silent)
                        AppendOutput($"Status Error {response.StatusCode}");
                }
            }
            catch
            {
                UpdateStatusLabel(false, "Connection Error");
            }
        }

        private void UpdateStatusLabel(bool isActive, string? overrideText = null)
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
                lblActiveStatus.Text = isActive ? "Status: Tracking Active" : "Status: Inactive";
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

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            var errors = ValidateInputs(out double ra, out double dec, out double lat, out double lon, out double interval);
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    "Please fix the following issues before starting:\r\n\r\n• " + string.Join("\r\n• ", errors),
                    "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string url = $"{serverBaseUrl}:5000/sidereal/start" +
                         $"?ra={ra.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                         $"&dec={dec.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                         $"&lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                         $"&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                         $"&interval={interval.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            AppendOutput($"Starting sidereal tracking (RA={ra}h, Dec={dec}°, Lat={lat}°, Lon={lon}°, Interval={interval}s)...");
            await SendGetRequest(url);
        }

        private async void BtnStop_Click(object? sender, EventArgs e)
        {
            string url = $"{serverBaseUrl}:5000/sidereal/stop";
            AppendOutput("Stopping sidereal tracking...");
            await SendGetRequest(url);
        }

        private async void BtnStatus_Click(object? sender, EventArgs e)
        {
            AppendOutput("Fetching status...");
            await RefreshStatusAsync(silent: false);
        }

        /// <summary>
        /// Validates all input fields. Populates out parameters only when valid.
        /// Returns a list of error messages (empty = all valid).
        /// </summary>
        private List<string> ValidateInputs(out double ra, out double dec, out double lat, out double lon, out double interval)
        {
            ra = dec = lat = lon = 0;
            interval = 5.0;
            var errors = new List<string>();

            // RA: decimal hours, 0 – 24
            if (!double.TryParse(txtRA.Text.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out ra))
                errors.Add("Right Ascension (RA) must be a decimal number (e.g. 5.575).");
            else if (ra < 0 || ra >= 24)
                errors.Add("Right Ascension (RA) must be in the range 0 – 24 hours.");

            // Dec: degrees, -90 – +90
            if (!double.TryParse(txtDec.Text.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out dec))
                errors.Add("Declination (Dec) must be a decimal number (e.g. -5.39).");
            else if (dec < -90 || dec > 90)
                errors.Add("Declination (Dec) must be in the range -90 – +90 degrees.");

            // Latitude: -90 – +90
            if (!double.TryParse(txtLat.Text.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out lat))
                errors.Add("Observer Latitude must be a decimal number (e.g. 32.08).");
            else if (lat < -90 || lat > 90)
                errors.Add("Observer Latitude must be in the range -90 – +90 degrees.");

            // Longitude: -180 – +180
            if (!double.TryParse(txtLon.Text.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out lon))
                errors.Add("Observer Longitude must be a decimal number (e.g. 34.78).");
            else if (lon < -180 || lon > 180)
                errors.Add("Observer Longitude must be in the range -180 – +180 degrees.");

            // Update interval: > 0
            if (!double.TryParse(txtInterval.Text.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out interval))
                errors.Add("Update Interval must be a positive decimal number (e.g. 5.0).");
            else if (interval <= 0)
                errors.Add("Update Interval must be greater than 0 seconds.");

            return errors;
        }

        private async Task SendGetRequest(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Try to pretty-print if JSON, otherwise show raw text
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<JsonElement>(body);
                        string formatted = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });
                        AppendOutput($"Success:\r\n{formatted}");
                    }
                    catch
                    {
                        AppendOutput($"Success: {body}");
                    }
                }
                else
                {
                    AppendOutput($"Error {(int)response.StatusCode} ({response.StatusCode}): {body}");
                }

                // Refresh status after any command
                await RefreshStatusAsync(silent: true);
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to connect: {ex.Message}");
                UpdateStatusLabel(false, "Connection Error");
            }
        }
    }
}
