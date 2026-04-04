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
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.lblIntervalHint = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStatus = new System.Windows.Forms.Button();
            this.lblActiveStatus = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.grpParams.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // grpParams  — spans full client width with 12px margin each side
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
            this.grpParams.Controls.Add(this.lblInterval);
            this.grpParams.Controls.Add(this.txtInterval);
            this.grpParams.Controls.Add(this.lblIntervalHint);
            this.grpParams.Location = new System.Drawing.Point(12, 12);
            this.grpParams.Name = "grpParams";
            this.grpParams.Size = new System.Drawing.Size(400, 200);
            this.grpParams.TabIndex = 0;
            this.grpParams.TabStop = false;
            this.grpParams.Text = "Tracking Parameters";
            //
            // Row layout inside grpParams:
            //   col 0 (label)  : x=8,  w=130
            //   col 1 (textbox): x=142, w=100
            //   col 2 (hint)   : x=248
            //
            // lblRA
            this.lblRA.AutoSize = true;
            this.lblRA.Location = new System.Drawing.Point(8, 28);
            this.lblRA.Name = "lblRA";
            this.lblRA.Text = "RA (hours):";
            // txtRA
            this.txtRA.Location = new System.Drawing.Point(145, 25);
            this.txtRA.Name = "txtRA";
            this.txtRA.Size = new System.Drawing.Size(95, 23);
            this.txtRA.TabIndex = 0;
            this.txtRA.Text = "0.0";
            // lblRAHint
            this.lblRAHint.AutoSize = true;
            this.lblRAHint.ForeColor = System.Drawing.Color.Gray;
            this.lblRAHint.Location = new System.Drawing.Point(248, 28);
            this.lblRAHint.Name = "lblRAHint";
            this.lblRAHint.Text = "0 – 24 h";
            // lblDec
            this.lblDec.AutoSize = true;
            this.lblDec.Location = new System.Drawing.Point(8, 62);
            this.lblDec.Name = "lblDec";
            this.lblDec.Text = "Dec (degrees):";
            // txtDec
            this.txtDec.Location = new System.Drawing.Point(145, 59);
            this.txtDec.Name = "txtDec";
            this.txtDec.Size = new System.Drawing.Size(95, 23);
            this.txtDec.TabIndex = 1;
            this.txtDec.Text = "0.0";
            // lblDecHint
            this.lblDecHint.AutoSize = true;
            this.lblDecHint.ForeColor = System.Drawing.Color.Gray;
            this.lblDecHint.Location = new System.Drawing.Point(248, 62);
            this.lblDecHint.Name = "lblDecHint";
            this.lblDecHint.Text = "-90 – +90°";
            // lblLat
            this.lblLat.AutoSize = true;
            this.lblLat.Location = new System.Drawing.Point(8, 96);
            this.lblLat.Name = "lblLat";
            this.lblLat.Text = "Latitude (degrees):";
            // txtLat
            this.txtLat.Location = new System.Drawing.Point(145, 93);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(95, 23);
            this.txtLat.TabIndex = 2;
            this.txtLat.Text = "0.0";
            // lblLatHint
            this.lblLatHint.AutoSize = true;
            this.lblLatHint.ForeColor = System.Drawing.Color.Gray;
            this.lblLatHint.Location = new System.Drawing.Point(248, 96);
            this.lblLatHint.Name = "lblLatHint";
            this.lblLatHint.Text = "-90 – +90°";
            // lblLon
            this.lblLon.AutoSize = true;
            this.lblLon.Location = new System.Drawing.Point(8, 130);
            this.lblLon.Name = "lblLon";
            this.lblLon.Text = "Longitude (degrees):";
            // txtLon
            this.txtLon.Location = new System.Drawing.Point(145, 127);
            this.txtLon.Name = "txtLon";
            this.txtLon.Size = new System.Drawing.Size(95, 23);
            this.txtLon.TabIndex = 3;
            this.txtLon.Text = "0.0";
            // lblLonHint
            this.lblLonHint.AutoSize = true;
            this.lblLonHint.ForeColor = System.Drawing.Color.Gray;
            this.lblLonHint.Location = new System.Drawing.Point(248, 130);
            this.lblLonHint.Name = "lblLonHint";
            this.lblLonHint.Text = "-180 – +180°";
            // lblInterval
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(8, 164);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Text = "Interval (seconds):";
            // txtInterval
            this.txtInterval.Location = new System.Drawing.Point(145, 161);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(95, 23);
            this.txtInterval.TabIndex = 4;
            this.txtInterval.Text = "5.0";
            // lblIntervalHint
            this.lblIntervalHint.AutoSize = true;
            this.lblIntervalHint.ForeColor = System.Drawing.Color.Gray;
            this.lblIntervalHint.Location = new System.Drawing.Point(248, 164);
            this.lblIntervalHint.Name = "lblIntervalHint";
            this.lblIntervalHint.Text = "> 0 s";
            //
            // pnlButtons — row of three buttons below the group box
            //
            this.pnlButtons.Controls.Add(this.btnStart);
            this.pnlButtons.Controls.Add(this.btnStop);
            this.pnlButtons.Controls.Add(this.btnStatus);
            this.pnlButtons.Location = new System.Drawing.Point(12, 220);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(400, 40);
            this.pnlButtons.TabIndex = 1;
            // btnStart
            this.btnStart.BackColor = System.Drawing.Color.DarkGreen;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStart.Location = new System.Drawing.Point(0, 0);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 35);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // btnStop
            this.btnStop.BackColor = System.Drawing.Color.DarkRed;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(130, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 35);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // btnStatus
            this.btnStatus.Location = new System.Drawing.Point(260, 0);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(120, 35);
            this.btnStatus.TabIndex = 2;
            this.btnStatus.Text = "Get Status";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.BtnStatus_Click);
            //
            // lblActiveStatus — full width below buttons
            //
            this.lblActiveStatus.AutoSize = false;
            this.lblActiveStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblActiveStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.lblActiveStatus.Location = new System.Drawing.Point(12, 263);
            this.lblActiveStatus.Name = "lblActiveStatus";
            this.lblActiveStatus.Size = new System.Drawing.Size(400, 20);
            this.lblActiveStatus.TabIndex = 2;
            this.lblActiveStatus.Text = "Status: Unknown";
            this.lblActiveStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtOutput — scrollable log below the status label
            //
            this.txtOutput.Location = new System.Drawing.Point(12, 290);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(400, 160);
            this.txtOutput.TabIndex = 3;
            //
            // SiderealTrackerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 462);
            this.Controls.Add(this.grpParams);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.lblActiveStatus);
            this.Controls.Add(this.txtOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SiderealTrackerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sidereal Tracker";
            this.grpParams.ResumeLayout(false);
            this.grpParams.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

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
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label lblIntervalHint;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.Label lblActiveStatus;
        private System.Windows.Forms.TextBox txtOutput;
    }
}
