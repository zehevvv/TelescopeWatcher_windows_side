namespace TelescopeWatcher
{
    partial class SiderealTrackerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblRaDecClock = new System.Windows.Forms.Label();
            this.grpCatalog = new System.Windows.Forms.GroupBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lstObjects = new System.Windows.Forms.ListBox();
            this.lblObjectInfo = new System.Windows.Forms.Label();
            this.btnUseSelected = new System.Windows.Forms.Button();
            this.grpParams = new System.Windows.Forms.GroupBox();
            this.lblRA = new System.Windows.Forms.Label();
            this.txtRA = new System.Windows.Forms.TextBox();
            this.lblRAHint = new System.Windows.Forms.Label();
            this.lblDec = new System.Windows.Forms.Label();
            this.txtDec = new System.Windows.Forms.TextBox();
            this.lblDecHint = new System.Windows.Forms.Label();
            this.lblLat = new System.Windows.Forms.Label();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.lblLatHint = new System.Windows.Forms.Label();
            this.lblLon = new System.Windows.Forms.Label();
            this.txtLon = new System.Windows.Forms.TextBox();
            this.lblLonHint = new System.Windows.Forms.Label();
            this.btnGetLocation = new System.Windows.Forms.Button();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.lblIntervalHint = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.txtSpeed = new System.Windows.Forms.TextBox();
            this.lblSpeedHint = new System.Windows.Forms.Label();
            this.btnSetSpeed = new System.Windows.Forms.Button();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStatus = new System.Windows.Forms.Button();
            this.lblActiveStatus = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.grpCatalog.SuspendLayout();
            this.grpParams.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // grpCatalog  (left column, x=12, w=230, h=430)
            //
            this.grpCatalog.Controls.Add(this.lblSearch);
            this.grpCatalog.Controls.Add(this.txtSearch);
            this.grpCatalog.Controls.Add(this.lstObjects);
            this.grpCatalog.Controls.Add(this.lblObjectInfo);
            this.grpCatalog.Controls.Add(this.btnUseSelected);
            this.grpCatalog.Location = new System.Drawing.Point(12, 12);
            this.grpCatalog.Name = "grpCatalog";
            this.grpCatalog.Size = new System.Drawing.Size(230, 430);
            this.grpCatalog.TabIndex = 10;
            this.grpCatalog.TabStop = false;
            this.grpCatalog.Text = "Celestial Object Catalog";
            //
            // lblSearch
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(8, 24);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Text = "Search:";
            // txtSearch  – live-filter as the user types
            this.txtSearch.Location = new System.Drawing.Point(8, 42);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(212, 23);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.PlaceholderText = "Name or alternate name…";
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            // lstObjects  – filtered results
            this.lstObjects.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.lstObjects.FormattingEnabled = true;
            this.lstObjects.ItemHeight = 16;
            this.lstObjects.Location = new System.Drawing.Point(8, 72);
            this.lstObjects.Name = "lstObjects";
            this.lstObjects.Size = new System.Drawing.Size(212, 256);
            this.lstObjects.TabIndex = 1;
            this.lstObjects.SelectedIndexChanged += new System.EventHandler(this.LstObjects_SelectedIndexChanged);
            this.lstObjects.DoubleClick += new System.EventHandler(this.LstObjects_DoubleClick);
            // lblObjectInfo  – shows RA / Dec of highlighted object
            this.lblObjectInfo.AutoSize = false;
            this.lblObjectInfo.ForeColor = System.Drawing.Color.DimGray;
            this.lblObjectInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblObjectInfo.Location = new System.Drawing.Point(8, 334);
            this.lblObjectInfo.Name = "lblObjectInfo";
            this.lblObjectInfo.Size = new System.Drawing.Size(212, 50);
            this.lblObjectInfo.Text = "";
            this.lblObjectInfo.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // btnUseSelected
            this.btnUseSelected.BackColor = System.Drawing.Color.SteelBlue;
            this.btnUseSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUseSelected.ForeColor = System.Drawing.Color.White;
            this.btnUseSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUseSelected.Location = new System.Drawing.Point(8, 390);
            this.btnUseSelected.Name = "btnUseSelected";
            this.btnUseSelected.Size = new System.Drawing.Size(212, 30);
            this.btnUseSelected.TabIndex = 2;
            this.btnUseSelected.Text = "Use Selected  (fill RA / Dec)";
            this.btnUseSelected.UseVisualStyleBackColor = false;
            this.btnUseSelected.Enabled = false;
            this.btnUseSelected.Click += new System.EventHandler(this.BtnUseSelected_Click);
            //
            // grpParams  (right column, x=254, w=390, h=255 — grown by 30px for btnGetLocation)
            //
            this.grpParams.Controls.Add(this.lblRA);
            this.grpParams.Controls.Add(this.txtRA);
            this.grpParams.Controls.Add(this.lblRAHint);
            this.grpParams.Controls.Add(this.lblDec);
            this.grpParams.Controls.Add(this.txtDec);
            this.grpParams.Controls.Add(this.lblDecHint);
            this.grpParams.Controls.Add(this.lblLat);
            this.grpParams.Controls.Add(this.txtLat);
            this.grpParams.Controls.Add(this.lblLatHint);
            this.grpParams.Controls.Add(this.lblLon);
            this.grpParams.Controls.Add(this.txtLon);
            this.grpParams.Controls.Add(this.lblLonHint);
            this.grpParams.Controls.Add(this.btnGetLocation);
            this.grpParams.Controls.Add(this.lblInterval);
            this.grpParams.Controls.Add(this.txtInterval);
            this.grpParams.Controls.Add(this.lblIntervalHint);
            this.grpParams.Controls.Add(this.lblSpeed);
            this.grpParams.Controls.Add(this.txtSpeed);
            this.grpParams.Controls.Add(this.lblSpeedHint);
            this.grpParams.Controls.Add(this.lblRaDecClock);
            this.grpParams.Location = new System.Drawing.Point(254, 12);
            this.grpParams.Name = "grpParams";
            this.grpParams.Size = new System.Drawing.Size(390, 294);
            this.grpParams.TabIndex = 0;
            this.grpParams.TabStop = false;
            this.grpParams.Text = "Tracking Parameters";
            //
            // Column layout inside grpParams:
            //   label x=8 w=140  |  textbox x=152 w=110  |  hint x=268
            // lblRA
            this.lblRA.AutoSize = true;
            this.lblRA.Location = new System.Drawing.Point(8, 28);
            this.lblRA.Name = "lblRA";
            this.lblRA.Text = "RA (hours):";
            // txtRA
            this.txtRA.Location = new System.Drawing.Point(152, 25);
            this.txtRA.Name = "txtRA";
            this.txtRA.Size = new System.Drawing.Size(110, 23);
            this.txtRA.TabIndex = 0;
            this.txtRA.Text = "0.0";
            this.txtRA.TextChanged += new System.EventHandler(this.TxtRADec_TextChanged);
            // lblRAHint
            this.lblRAHint.AutoSize = true;
            this.lblRAHint.ForeColor = System.Drawing.Color.Gray;
            this.lblRAHint.Location = new System.Drawing.Point(268, 28);
            this.lblRAHint.Name = "lblRAHint";
            this.lblRAHint.Text = "0 – 24 h";
            // lblDec
            this.lblDec.AutoSize = true;
            this.lblDec.Location = new System.Drawing.Point(8, 62);
            this.lblDec.Name = "lblDec";
            this.lblDec.Text = "Dec (degrees):";
            // txtDec
            this.txtDec.Location = new System.Drawing.Point(152, 59);
            this.txtDec.Name = "txtDec";
            this.txtDec.Size = new System.Drawing.Size(110, 23);
            this.txtDec.TabIndex = 1;
            this.txtDec.Text = "0.0";
            this.txtDec.TextChanged += new System.EventHandler(this.TxtRADec_TextChanged);
            // lblDecHint
            this.lblDecHint.AutoSize = true;
            this.lblDecHint.ForeColor = System.Drawing.Color.Gray;
            this.lblDecHint.Location = new System.Drawing.Point(268, 62);
            this.lblDecHint.Name = "lblDecHint";
            this.lblDecHint.Text = "-90 – +90°";
            // lblLat
            this.lblLat.AutoSize = true;
            this.lblLat.Location = new System.Drawing.Point(8, 96);
            this.lblLat.Name = "lblLat";
            this.lblLat.Text = "Observer Latitude:";
            // txtLat
            this.txtLat.Location = new System.Drawing.Point(152, 93);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(110, 23);
            this.txtLat.TabIndex = 2;
            this.txtLat.Text = "32.2667";
            // lblLatHint
            this.lblLatHint.AutoSize = true;
            this.lblLatHint.ForeColor = System.Drawing.Color.Gray;
            this.lblLatHint.Location = new System.Drawing.Point(268, 96);
            this.lblLatHint.Name = "lblLatHint";
            this.lblLatHint.Text = "-90 – +90°";
            // lblLon
            this.lblLon.AutoSize = true;
            this.lblLon.Location = new System.Drawing.Point(8, 130);
            this.lblLon.Name = "lblLon";
            this.lblLon.Text = "Observer Longitude:";
            // txtLon
            this.txtLon.Location = new System.Drawing.Point(152, 127);
            this.txtLon.Name = "txtLon";
            this.txtLon.Size = new System.Drawing.Size(110, 23);
            this.txtLon.TabIndex = 3;
            this.txtLon.Text = "34.8833";
            // lblLonHint
            this.lblLonHint.AutoSize = true;
            this.lblLonHint.ForeColor = System.Drawing.Color.Gray;
            this.lblLonHint.Location = new System.Drawing.Point(268, 130);
            this.lblLonHint.Name = "lblLonHint";
            this.lblLonHint.Text = "-180 – +180°";
            // btnGetLocation
            this.btnGetLocation.Location = new System.Drawing.Point(8, 157);
            this.btnGetLocation.Name = "btnGetLocation";
            this.btnGetLocation.Size = new System.Drawing.Size(254, 26);
            this.btnGetLocation.TabIndex = 4;
            this.btnGetLocation.Text = "Get My Location (via Internet)";
            this.btnGetLocation.UseVisualStyleBackColor = true;
            this.btnGetLocation.Enabled = false;
            this.btnGetLocation.Click += new System.EventHandler(this.BtnGetLocation_Click);
            // lblInterval
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(8, 196);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Text = "Update Interval:";
            // txtInterval
            this.txtInterval.Location = new System.Drawing.Point(152, 193);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(110, 23);
            this.txtInterval.TabIndex = 5;
            this.txtInterval.Text = "5.0";
            // lblIntervalHint
            this.lblIntervalHint.AutoSize = true;
            this.lblIntervalHint.ForeColor = System.Drawing.Color.Gray;
            this.lblIntervalHint.Location = new System.Drawing.Point(268, 196);
            this.lblIntervalHint.Name = "lblIntervalHint";
            this.lblIntervalHint.Text = "seconds, > 0";
            // lblSpeed
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(8, 230);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Text = "Speed Factor:";
            // txtSpeed
            this.txtSpeed.Location = new System.Drawing.Point(152, 227);
            this.txtSpeed.Name = "txtSpeed";
            this.txtSpeed.Size = new System.Drawing.Size(110, 23);
            this.txtSpeed.TabIndex = 6;
            this.txtSpeed.Text = "1.0";
            // lblSpeedHint
            this.lblSpeedHint.AutoSize = true;
            this.lblSpeedHint.ForeColor = System.Drawing.Color.Gray;
            this.lblSpeedHint.Location = new System.Drawing.Point(268, 230);
            this.lblSpeedHint.Name = "lblSpeedHint";
            this.lblSpeedHint.Text = "1.0 = sidereal";
            //
            // lblRaDecClock — sexagesimal preview, spans full width below RA/Dec rows
            //
            this.lblRaDecClock.AutoSize = false;
            this.lblRaDecClock.Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblRaDecClock.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblRaDecClock.Location = new System.Drawing.Point(8, 264);
            this.lblRaDecClock.Name = "lblRaDecClock";
            this.lblRaDecClock.Size = new System.Drawing.Size(372, 20);
            this.lblRaDecClock.Text = "";
            this.lblRaDecClock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlButtons  (shifted down to match grown grpParams)
            //
            this.pnlButtons.Controls.Add(this.btnStart);
            this.pnlButtons.Controls.Add(this.btnStop);
            this.pnlButtons.Controls.Add(this.btnStatus);
            this.pnlButtons.Controls.Add(this.btnSetSpeed);
            this.pnlButtons.Location = new System.Drawing.Point(254, 314);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(390, 40);
            this.pnlButtons.TabIndex = 1;
            // btnStart
            this.btnStart.BackColor = System.Drawing.Color.DarkGreen;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStart.Location = new System.Drawing.Point(0, 0);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(90, 35);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // btnStop
            this.btnStop.BackColor = System.Drawing.Color.DarkRed;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(98, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(90, 35);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // btnStatus
            this.btnStatus.Location = new System.Drawing.Point(196, 0);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(90, 35);
            this.btnStatus.TabIndex = 2;
            this.btnStatus.Text = "Get Status";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.BtnStatus_Click);
            // btnSetSpeed
            this.btnSetSpeed.Location = new System.Drawing.Point(294, 0);
            this.btnSetSpeed.Name = "btnSetSpeed";
            this.btnSetSpeed.Size = new System.Drawing.Size(96, 35);
            this.btnSetSpeed.TabIndex = 3;
            this.btnSetSpeed.Text = "Set Speed";
            this.btnSetSpeed.UseVisualStyleBackColor = true;
            this.btnSetSpeed.Click += new System.EventHandler(this.BtnSetSpeed_Click);
            //
            // lblActiveStatus  (shifted down to match)
            //
            this.lblActiveStatus.AutoSize = false;
            this.lblActiveStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblActiveStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.lblActiveStatus.Location = new System.Drawing.Point(254, 357);
            this.lblActiveStatus.Name = "lblActiveStatus";
            this.lblActiveStatus.Size = new System.Drawing.Size(390, 20);
            this.lblActiveStatus.TabIndex = 2;
            this.lblActiveStatus.Text = "Status: Unknown";
            this.lblActiveStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtOutput  (full width, below everything)
            //
            this.txtOutput.Location = new System.Drawing.Point(12, 450);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(632, 140);
            this.txtOutput.TabIndex = 3;
            //
            // SiderealTrackerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 602);
            this.Controls.Add(this.grpCatalog);
            this.Controls.Add(this.grpParams);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.lblActiveStatus);
            this.Controls.Add(this.txtOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SiderealTrackerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sidereal Tracker";
            this.grpCatalog.ResumeLayout(false);
            this.grpCatalog.PerformLayout();
            this.grpParams.ResumeLayout(false);
            this.grpParams.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpCatalog;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListBox lstObjects;
        private System.Windows.Forms.Label lblObjectInfo;
        private System.Windows.Forms.Button btnUseSelected;
        private System.Windows.Forms.GroupBox grpParams;
        private System.Windows.Forms.Label lblRA;
        private System.Windows.Forms.TextBox txtRA;
        private System.Windows.Forms.Label lblRAHint;
        private System.Windows.Forms.Label lblDec;
        private System.Windows.Forms.TextBox txtDec;
        private System.Windows.Forms.Label lblDecHint;
        private System.Windows.Forms.Label lblLat;
        private System.Windows.Forms.TextBox txtLat;
        private System.Windows.Forms.Label lblLatHint;
        private System.Windows.Forms.Label lblLon;
        private System.Windows.Forms.TextBox txtLon;
        private System.Windows.Forms.Label lblLonHint;
        private System.Windows.Forms.Button btnGetLocation;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label lblIntervalHint;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.TextBox txtSpeed;
        private System.Windows.Forms.Label lblSpeedHint;
        private System.Windows.Forms.Button btnSetSpeed;
        private System.Windows.Forms.Label lblRaDecClock;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.Label lblActiveStatus;
        private System.Windows.Forms.TextBox txtOutput;
    }
}
