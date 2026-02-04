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
        
        // Configuration fields
        private List<CameraControl> cameraControls = new List<CameraControl>();
        private Dictionary<string, object> originalValues = new Dictionary<string, object>();
        private Dictionary<string, GroupBox> controlGroupBoxes = new Dictionary<string, GroupBox>();
        private string currentFilePath = "";
        private bool isDirty = false;
        private bool isLoadingUI = false; // Flag to prevent dirty checks during load

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
            controlGroupBoxes.Clear();
            cameraControls.Clear();

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
                    cameraControls = controls;
                    
                    // Capture baseline
                    originalValues.Clear();
                    foreach (var c in cameraControls)
                    {
                        originalValues[c.Name] = c.Value;
                    }
                    
                    isLoadingUI = true;
                    foreach (var control in controls)
                    {
                        AddControlToUI(control);
                    }
                    isLoadingUI = false;
                    
                    lblStatus.Text = $"Loaded {controls.Count} controls";
                    UpdateFileLabel();
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
            
            controlGroupBoxes[control.Name] = gb; // Store for updating color
            
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
                        string val = cb.Checked ? "1" : "0";
                        SendControlUpdate(cb.Tag.ToString(), val);
                        CheckDirty(cb.Tag.ToString(), cb.Checked ? 1 : 0); // Check int value
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
                        CheckDirty(t.Tag.ToString(), t.Value);
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
                             CheckDirty(name, t.Text);
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

        private async void BtnSetDefault_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Resetting controls to defaults...";
                string url = $"{apiUrl}/cam/{cameraEndpoint}/reset_controls";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    lblStatus.Text = "Controls reset successfully";
                    // Reload controls to reflect new default values
                    await LoadControls();
                }
                else
                {
                    lblStatus.Text = "Failed to reset controls";
                    MessageBox.Show("Failed to reset controls. Server returned " + response.StatusCode, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error resetting controls";
                MessageBox.Show($"Error resetting controls: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckDirty(string name, object newValue)
        {
            if (isLoadingUI) return;

            // Attempt to update the internal list model
            var control = cameraControls.Find(c => c.Name == name);
            if (control != null)
            {
                // Ensure type consistency if possible
                if (newValue is int i) control.Value = i;
                else if (newValue is string s) 
                {
                    // Try parse if original was int? No, stick to raw for now
                     control.Value = newValue;
                }
                else control.Value = newValue;
            }

            if (originalValues.ContainsKey(name))
            {
                var original = originalValues[name];
                bool isChanged = false;

                // Simple comparison
                string s1 = original?.ToString() ?? "";
                string s2 = newValue?.ToString() ?? "";
                if (s1 != s2) isChanged = true;

                if (controlGroupBoxes.ContainsKey(name))
                {
                    var gb = controlGroupBoxes[name];
                    if (isChanged)
                    {
                        gb.ForeColor = Color.Red;
                    }
                    else
                    {
                        gb.ForeColor = Color.Black; // Reset
                    }
                }
            }
            
            // Check global dirty state
            isDirty = CheckAllDirty();
            UpdateFileLabel();
        }

        private bool CheckAllDirty()
        {
            foreach(var kvp in originalValues)
            {
                var control = cameraControls.Find(c => c.Name == kvp.Key);
                if (control != null)
                {
                    string s1 = kvp.Value?.ToString() ?? "";
                    string s2 = control.Value?.ToString() ?? "";
                    if (s1 != s2) return true;
                }
            }
            return false;
        }

        private void UpdateFileLabel()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath) ? "No file loaded" : System.IO.Path.GetFileName(currentFilePath);
            if (isDirty)
            {
                lblCurrentFile.Text = $"{fileName} *";
                lblCurrentFile.ForeColor = Color.Red;
            }
            else
            {
                lblCurrentFile.Text = fileName;
                lblCurrentFile.ForeColor = Color.Black;
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON Files|*.json|All Files|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        
                        // Create configuration object with camera name
                        var config = new CameraConfiguration
                        {
                            CameraName = cameraEndpoint,
                            Controls = cameraControls
                        };
                        
                        string json = JsonSerializer.Serialize(config, options);
                        await System.IO.File.WriteAllTextAsync(sfd.FileName, json);
                        
                        currentFilePath = sfd.FileName;
                        
                        // Update baseline to current
                        originalValues.Clear();
                        foreach(var c in cameraControls)
                        {
                            originalValues[c.Name] = c.Value;
                            if (controlGroupBoxes.ContainsKey(c.Name))
                                controlGroupBoxes[c.Name].ForeColor = Color.Black;
                        }
                        
                        isDirty = false;
                        UpdateFileLabel();
                        lblStatus.Text = "Configuration saved.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving file: " + ex.Message);
                    }
                }
            }
        }

        private async void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files|*.json|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = await System.IO.File.ReadAllTextAsync(ofd.FileName);
                        
                        // Try deserialize as configuration object
                        CameraConfiguration config = null;
                        try 
                        {
                            config = JsonSerializer.Deserialize<CameraConfiguration>(json, new JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                        }
                        catch
                        {
                            // Ignored - likely invalid format or legacy file
                        }

                        if (config != null && !string.IsNullOrEmpty(config.CameraName) && config.Controls != null)
                        {
                            // Validate camera name
                            if (!string.Equals(config.CameraName, cameraEndpoint, StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show($"This configuration file is for camera '{config.CameraName}', but you are currently configuring '{cameraEndpoint}'. Load cancelled to prevent applying incompatible settings.", 
                                    "Camera Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            
                            cameraControls = config.Controls;
                            currentFilePath = ofd.FileName;
                            
                            // Apply to camera
                            lblStatus.Text = "Applying settings...";
                            foreach(var c in cameraControls)
                            {
                                string val = c.Value?.ToString() ?? "";
                                string url = $"{apiUrl}/cam/{cameraEndpoint}/set_control?name={c.Name}&value={val}";
                                await httpClient.GetAsync(url);
                            }
                            
                            // Rebuild UI
                            isLoadingUI = true;
                            flowLayoutPanelControls.Controls.Clear();
                            valueLabels.Clear();
                            controlGroupBoxes.Clear();
                            
                            // Capture new baseline FROM FILE
                            originalValues.Clear();
                            foreach (var c in cameraControls)
                            {
                                originalValues[c.Name] = c.Value;
                                AddControlToUI(c);
                            }
                            isLoadingUI = false;

                            isDirty = false;
                            UpdateFileLabel();
                            lblStatus.Text = "Configuration loaded and applied.";
                        }
                        else
                        {
                            MessageBox.Show("Invalid configuration file format. The file must contain the camera identifier to ensure compatibility.", 
                                "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading file: " + ex.Message);
                    }
                }
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

    public class CameraConfiguration
    {
        public string CameraName { get; set; }
        public List<CameraControl> Controls { get; set; }
    }
}
