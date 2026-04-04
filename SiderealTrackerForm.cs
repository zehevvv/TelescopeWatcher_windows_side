using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
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

        // Filtered list backing the ListBox
        private List<CelestialObject> _filteredObjects = new();

        public SiderealTrackerForm(string serverUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverUrl;

            PopulateCatalog("");

            // Trigger the clock label with the default 0.0 / 0.0 values
            TxtRADec_TextChanged(null, EventArgs.Empty);

            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 2000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            UpdateGetLocationButton();

            this.FormClosing += SiderealTrackerForm_FormClosing;
        }

        // ??????????????????????????????????????????????????????????????
        // Catalog helpers
        // ??????????????????????????????????????????????????????????????

        private void PopulateCatalog(string search)
        {
            _filteredObjects = CelestialCatalog.Search(search).ToList();

            lstObjects.BeginUpdate();
            lstObjects.Items.Clear();
            foreach (var obj in _filteredObjects)
            {
                // Format: "[Type] Name (Alternate)"
                lstObjects.Items.Add($"[{obj.TypeTag,-4}]  {obj.DisplayName}");
            }
            lstObjects.EndUpdate();

            btnUseSelected.Enabled = false;
            lblObjectInfo.Text = _filteredObjects.Count == 0
                ? "No matches found."
                : $"{_filteredObjects.Count} object(s)";
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            PopulateCatalog(txtSearch.Text);
        }

        /// <summary>
        /// Converts a decimal RA/Dec value to sexagesimal clock format and updates lblRaDecClock.
        /// Called whenever txtRA or txtDec changes.
        /// </summary>
        private void TxtRADec_TextChanged(object? sender, EventArgs e)
        {
            bool raOk  = double.TryParse(txtRA.Text.Trim(),  System.Globalization.NumberStyles.Float,
                             System.Globalization.CultureInfo.InvariantCulture, out double ra)
                         && ra >= 0 && ra < 24;
            bool decOk = double.TryParse(txtDec.Text.Trim(), System.Globalization.NumberStyles.Float,
                             System.Globalization.CultureInfo.InvariantCulture, out double dec)
                         && dec >= -90 && dec <= 90;

            if (raOk && decOk)
            {
                lblRaDecClock.Text = $"RA  {ToHms(ra)}    Dec  {ToDms(dec)}";
                lblRaDecClock.ForeColor = System.Drawing.Color.SteelBlue;
            }
            else
            {
                lblRaDecClock.Text = raOk || decOk ? "—" : "";
                lblRaDecClock.ForeColor = System.Drawing.Color.Gray;
            }
        }

        // "5.5756"  ?  "05h 34m 32s"
        private static string ToHms(double hours)
        {
            hours = Math.Abs(hours);
            int h  = (int)hours;
            int m  = (int)((hours - h) * 60);
            int s  = (int)(((hours - h) * 60 - m) * 60);
            return $"{h:D2}h {m:D2}m {s:D2}s";
        }

        // "22.8453"  ?  "+22° 50' 43""
        private static string ToDms(double degrees)
        {
            char sign = degrees >= 0 ? '+' : '-';
            degrees = Math.Abs(degrees);
            int d  = (int)degrees;
            int m  = (int)((degrees - d) * 60);
            int s  = (int)(((degrees - d) * 60 - m) * 60);
            return $"{sign}{d:D2}° {m:D2}' {s:D2}\"";
        }

        private void LstObjects_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int idx = lstObjects.SelectedIndex;
            if (idx < 0 || idx >= _filteredObjects.Count)
            {
                lblObjectInfo.Text = "";
                btnUseSelected.Enabled = false;
                return;
            }

            var obj = _filteredObjects[idx];

            if (obj.IsLive)
            {
                try
                {
                    var (ra, dec) = obj.GetCurrentCoordinates();
                    lblObjectInfo.Text =
                        $"{obj.DisplayName}  ? live\n" +
                        $"Type : {obj.Type}\n" +
                        $"RA   : {ra:F4} h  (now)\n" +
                        $"Dec  : {dec:+0.0000;-0.0000}°  (now)";
                }
                catch (Exception ex)
                {
                    lblObjectInfo.Text = $"Error computing position:\n{ex.Message}";
                }
            }
            else
            {
                lblObjectInfo.Text =
                    $"{obj.DisplayName}\n" +
                    $"Type : {obj.Type}\n" +
                    $"RA   : {obj.RA:F4} h\n" +
                    $"Dec  : {obj.Dec:+0.0000;-0.0000}°";
            }

            btnUseSelected.Enabled = true;
        }

        private void LstObjects_DoubleClick(object? sender, EventArgs e)
        {
            BtnUseSelected_Click(sender, e);
        }

        private void BtnUseSelected_Click(object? sender, EventArgs e)
        {
            int idx = lstObjects.SelectedIndex;
            if (idx < 0 || idx >= _filteredObjects.Count) return;

            var obj = _filteredObjects[idx];
            try
            {
                var (ra, dec) = obj.GetCurrentCoordinates();
                txtRA.Text  = ra.ToString("F4", CultureInfo.InvariantCulture);
                txtDec.Text = dec.ToString("F4", CultureInfo.InvariantCulture);

                string liveNote = obj.IsLive ? " (live position)" : " (J2000)";
                AppendOutput($"Loaded '{obj.DisplayName}'{liveNote} ? RA={ra:F4} h, Dec={dec:+0.0000;-0.0000}°");
            }
            catch (Exception ex)
            {
                AppendOutput($"Error getting coordinates for '{obj.DisplayName}': {ex.Message}");
            }
        }

        // ??????????????????????????????????????????????????????????????
        // Status polling
        // ??????????????????????????????????????????????????????????????

        private void SiderealTrackerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
        }

        private async void StatusTimer_Tick(object? sender, EventArgs e)
        {
            UpdateGetLocationButton();
            await RefreshStatusAsync(silent: true);
        }

        // ??????????????????????????????????????????????????????????????
        // Location helpers
        // ??????????????????????????????????????????????????????????????

        private static bool IsInternetAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable() &&
                   NetworkInterface.GetAllNetworkInterfaces()
                       .Any(n => n.OperationalStatus == OperationalStatus.Up
                              && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                              && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
        }

        private void UpdateGetLocationButton()
        {
            bool online = IsInternetAvailable();
            if (btnGetLocation.InvokeRequired)
                btnGetLocation.Invoke(new Action(() => btnGetLocation.Enabled = online));
            else
                btnGetLocation.Enabled = online;
        }

        private async void BtnGetLocation_Click(object? sender, EventArgs e)
        {
            btnGetLocation.Enabled = false;
            btnGetLocation.Text = "Locating…";
            try
            {
                // ip-api.com returns JSON with lat/lon based on the public IP; no API key needed
                string json = await httpClient.GetStringAsync("http://ip-api.com/json/?fields=status,lat,lon,city,country");
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var status) && status.GetString() == "success"
                    && root.TryGetProperty("lat", out var latEl)
                    && root.TryGetProperty("lon", out var lonEl))
                {
                    double lat = latEl.GetDouble();
                    double lon = lonEl.GetDouble();

                    string city = root.TryGetProperty("city", out var cityEl) ? cityEl.GetString() ?? "" : "";
                    string country = root.TryGetProperty("country", out var countryEl) ? countryEl.GetString() ?? "" : "";

                    txtLat.Text = lat.ToString("F4", CultureInfo.InvariantCulture);
                    txtLon.Text = lon.ToString("F4", CultureInfo.InvariantCulture);

                    AppendOutput($"Location detected: {city}, {country}  ?  Lat={lat:F4}°, Lon={lon:F4}°");
                }
                else
                {
                    AppendOutput("Could not determine location from IP.");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Location lookup failed: {ex.Message}");
            }
            finally
            {
                btnGetLocation.Text = "Get My Location (via Internet)";
                UpdateGetLocationButton();
            }
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

        // ??????????????????????????????????????????????????????????????
        // Button handlers
        // ??????????????????????????????????????????????????????????????

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
                         $"?ra={ra.ToString(CultureInfo.InvariantCulture)}" +
                         $"&dec={dec.ToString(CultureInfo.InvariantCulture)}" +
                         $"&lat={lat.ToString(CultureInfo.InvariantCulture)}" +
                         $"&lon={lon.ToString(CultureInfo.InvariantCulture)}" +
                         $"&interval={interval.ToString(CultureInfo.InvariantCulture)}";

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

        // ??????????????????????????????????????????????????????????????
        // Validation
        // ??????????????????????????????????????????????????????????????

        private List<string> ValidateInputs(out double ra, out double dec,
                                            out double lat, out double lon, out double interval)
        {
            ra = dec = lat = lon = 0;
            interval = 5.0;
            var errors = new List<string>();

            if (!double.TryParse(txtRA.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out ra))
                errors.Add("Right Ascension (RA) must be a decimal number (e.g. 5.575).");
            else if (ra < 0 || ra >= 24)
                errors.Add("Right Ascension (RA) must be in the range 0 – 24 hours.");

            if (!double.TryParse(txtDec.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out dec))
                errors.Add("Declination (Dec) must be a decimal number (e.g. -5.39).");
            else if (dec < -90 || dec > 90)
                errors.Add("Declination (Dec) must be in the range -90 – +90 degrees.");

            if (!double.TryParse(txtLat.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lat))
                errors.Add("Observer Latitude must be a decimal number (e.g. 32.08).");
            else if (lat < -90 || lat > 90)
                errors.Add("Observer Latitude must be in the range -90 – +90 degrees.");

            if (!double.TryParse(txtLon.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                errors.Add("Observer Longitude must be a decimal number (e.g. 34.78).");
            else if (lon < -180 || lon > 180)
                errors.Add("Observer Longitude must be in the range -180 – +180 degrees.");

            if (!double.TryParse(txtInterval.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out interval))
                errors.Add("Update Interval must be a positive decimal number (e.g. 5.0).");
            else if (interval <= 0)
                errors.Add("Update Interval must be greater than 0 seconds.");

            return errors;
        }

        // ??????????????????????????????????????????????????????????????
        // HTTP
        // ??????????????????????????????????????????????????????????????

        private async Task SendGetRequest(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
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
