using System;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class PlateSolverForm : Form
    {
        private readonly string serverBaseUrl;
        private static readonly HttpClient httpClient = new HttpClient() 
        { 
            Timeout = TimeSpan.FromSeconds(60) // High timeout since solving can take 10s of seconds
        };

        public PlateSolverForm(string serverUrl)
        {
            InitializeComponent();
            this.serverBaseUrl = serverUrl;
            comboCamera.SelectedIndex = 0; // Default to "hd"
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            string selectedCamera = comboCamera.SelectedItem?.ToString() ?? "hd";
            string url = $"{serverBaseUrl}:5000/cam/solve?camera={selectedCamera}";

            txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] Solving for camera '{selectedCamera}'...\r\n");
            btnSolve.Enabled = false;

            // Clear previous results
            txtRa.Text = "";
            txtDec.Text = "";
            txtRotation.Text = "";

            try
            {
                var response = await httpClient.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PlateSolverResult>(json);
                    if (result != null)
                    {
                        if (result.Success)
                        {
                            txtRa.Text = result.RaDeg.ToString("F5");
                            txtDec.Text = result.DecDeg.ToString("F5");
                            txtRotation.Text = result.Rotation.ToString("F5");
                            txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] Success. RA: {result.RaDeg:F5}, DEC: {result.DecDeg:F5}, Rot: {result.Rotation:F5}\r\n");
                        }
                        else
                        {
                            string errorMsg = string.IsNullOrEmpty(result.Error) ? "No solution found" : result.Error;
                            txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] Failed: {errorMsg}\r\n");
                        }
                    }
                    else
                    {
                        txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] Failed to parse JSON response.\r\n");
                    }
                }
                else
                {
                    txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] HTTP Error: {response.StatusCode} - {json}\r\n");
                }
            }
            catch (Exception ex)
            {
                txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}\r\n");
            }
            finally
            {
                btnSolve.Enabled = true;
            }
        }
    }
}