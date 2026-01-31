using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class CameraSettingsForm : Form
    {
        private readonly string apiUrl;
        private readonly string cameraEndpoint;
        private readonly HttpClient httpClient;
        private System.Windows.Forms.Timer debounceTimer;
        private string pendingControlName;
        private string pendingControlValue;
        
        // Dictionary to keep track of value labels for sliders
        private Dictionary<string, Label> valueLabels = new Dictionary<string, Label>();

        public CameraSettingsForm(string serverBaseUrl, string cameraEndpoint)
        {
            InitializeComponent();
            
            this.cameraEndpoint = cameraEndpoint;
            
            try
            {
                var uri = new Uri(serverBaseUrl);
                this.apiUrl = $"{uri.Scheme}://{uri.Host}:5000";
            }
            catch
            {
                this.apiUrl = $"{serverBaseUrl}:5000";
            }

            this.httpClient = new HttpClient();
            this.httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            this.Text = $"Camera Settings - {cameraEndpoint.ToUpper()}";

            debounceTimer = new System.Windows.Forms.Timer();
            debounceTimer.Interval = 300; // 300ms debounce
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        private async void CameraSettingsForm_Load(object sender, EventArgs e)
        {
            await LoadControls();
        }

        private async Task LoadControls()
        {
            lblStatus.Text = "Loading controls...";
            flowLayoutPanelControls.Controls.Clear();
            valueLabels.Clear();

            try
            {
                string url = $"{apiUrl}/cam/{cameraEndpoint}/controls";
                var response = await httpClient.GetStringAsync(url);
                
                var controls = JsonSerializer.Deserialize<List<CameraControl>>(response, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (controls != null)
                {
                    foreach (var control in controls)
                    {
                        AddControlToUI(control);
                    }
                    lblStatus.Text = $"Loaded {controls.Count} controls";
                }
                else
                {
                    lblStatus.Text = "No controls found";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading controls";
                MessageBox.Show($"Failed to load controls: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddControlToUI(CameraControl control)
        {
            GroupBox gb = new GroupBox();
            gb.Text = control.Name;
            gb.Width = flowLayoutPanelControls.Width - 40; // Full width with some padding
            gb.Height = 70;
            
            // Layout logic based on type and min/max
            if (control.Min.HasValue && control.Max.HasValue)
            {
                // Is boolean? (0-1)
                if (control.Min == 0 && control.Max == 1 && (control.Type == "bool" || control.Type == "int"))
                {
                    CheckBox chk = new CheckBox();
                    chk.Text = "Enabled";
                    chk.Location = new Point(10, 25);
                    chk.Checked = Convert.ToInt32(control.Value.ToString()) == 1;
                    chk.Tag = control.Name;
                    chk.CheckedChanged += (s, e) => 
                    {
                        var cb = s as CheckBox;
                        SendControlUpdate(cb.Tag.ToString(), cb.Checked ? "1" : "0");
                    };
                    gb.Controls.Add(chk);
                }
                else
                {
                    // Slider
                    TrackBar tb = new TrackBar();
                    tb.Minimum = control.Min.Value;
                    tb.Maximum = control.Max.Value;
                    tb.Value = Math.Max(tb.Minimum, Math.Min(tb.Maximum, Convert.ToInt32(control.Value.ToString())));
                    tb.SmallChange = control.Step ?? 1;
                    tb.LargeChange = (control.Max.Value - control.Min.Value) / 10;
                    if (tb.LargeChange < 1) tb.LargeChange = 1;
                    
                    tb.TickFrequency = (control.Max.Value - control.Min.Value) / 10;
                    if (tb.TickFrequency < 1) tb.TickFrequency = 1;

                    tb.Location = new Point(10, 20);
                    tb.Width = gb.Width - 80;
                    tb.Tag = control.Name;
                    
                    Label valLbl = new Label();
                    valLbl.Text = tb.Value.ToString();
                    valLbl.Location = new Point(tb.Right + 5, 25);
                    valLbl.AutoSize = true;
                    
                    valueLabels[control.Name] = valLbl;

                    tb.Scroll += (s, e) => 
                    {
                        var t = s as TrackBar;
                        if (valueLabels.ContainsKey(t.Tag.ToString()))
                        {
                            valueLabels[t.Tag.ToString()].Text = t.Value.ToString();
                        }
                        QueueControlUpdate(t.Tag.ToString(), t.Value.ToString());
                    };

                    gb.Controls.Add(tb);
                    gb.Controls.Add(valLbl);
                }
            }
            else
            {
                // Fallback: TextBox
                TextBox txt = new TextBox();
                txt.Text = control.Value?.ToString() ?? "";
                txt.Location = new Point(10, 25);
                txt.Width = 200;
                txt.Tag = control.Name;
                
                Button btnSet = new Button();
                btnSet.Text = "Set";
                btnSet.Location = new Point(txt.Right + 10, 23);
                btnSet.Tag = control.Name; // Store control name in button tag too
                
                // We need to capture the associated textbox
                btnSet.Click += (s, e) => 
                {
                    var btn = s as Button;
                    var name = btn.Tag.ToString();
                    // Find sibling textbox
                    var parent = btn.Parent;
                    foreach(Control c in parent.Controls) {
                        if (c is TextBox t && t.Tag.ToString() == name) {
                             SendControlUpdate(name, t.Text);
                             break;
                        }
                    }
                };
                
                gb.Controls.Add(txt);
                gb.Controls.Add(btnSet);
            }

            flowLayoutPanelControls.Controls.Add(gb);
        }

        private void QueueControlUpdate(string name, string value)
        {
            pendingControlName = name;
            pendingControlValue = value;
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            if (!string.IsNullOrEmpty(pendingControlName))
            {
                SendControlUpdate(pendingControlName, pendingControlValue);
                pendingControlName = null;
            }
        }

        private async void SendControlUpdate(string name, string value)
        {
            try
            {
                lblStatus.Text = $"Setting {name} to {value}...";
                string url = $"{apiUrl}/cam/{cameraEndpoint}/set_control?name={name}&value={value}";
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    lblStatus.Text = $"Set {name} success";
                }
                else
                {
                    lblStatus.Text = $"Failed to set {name}";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error sending update";
                System.Diagnostics.Debug.WriteLine($"Error setting control: {ex.Message}");
            }
        }
    }

    public class CameraControl
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("min")]
        public int? Min { get; set; }

        [JsonPropertyName("max")]
        public int? Max { get; set; }

        [JsonPropertyName("step")]
        public int? Step { get; set; }

        [JsonPropertyName("default")]
        public int? Default { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}
