namespace TelescopeWatcher
{
    partial class TelescopeControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblControl = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPortInfo = new System.Windows.Forms.Label();
            this.grpTimeBetweenSteps = new System.Windows.Forms.GroupBox();
            this.lblStepsPerSecondValue = new System.Windows.Forms.Label();
            this.lblTimeValue = new System.Windows.Forms.Label();
            this.trackBarStepsPerSecond = new System.Windows.Forms.TrackBar();
            this.lblStepsPerSecond = new System.Windows.Forms.Label();
            this.grpFocusControl = new System.Windows.Forms.GroupBox();
            this.lblFocusSpeedValue = new System.Windows.Forms.Label();
            this.trackBarFocusSpeed = new System.Windows.Forms.TrackBar();
            this.lblFocusSpeed = new System.Windows.Forms.Label();
            this.btnFocusDecrease = new System.Windows.Forms.Button();
            this.btnFocusIncrease = new System.Windows.Forms.Button();
            this.grpVideoStream = new System.Windows.Forms.GroupBox();
            this.lblVideoStatus = new System.Windows.Forms.Label();
            this.btnVideoStop = new System.Windows.Forms.Button();
            this.btnVideoStart = new System.Windows.Forms.Button();
            this.grpTimeBetweenSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).BeginInit();
            this.grpFocusControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).BeginInit();
            this.grpVideoStream.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUp
            // 
            this.btnUp.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUp.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnUp.FlatAppearance.BorderSize = 2;
            this.btnUp.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnUp.Location = new System.Drawing.Point(450, 60);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(60, 50);
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "UP";
            this.btnUp.UseVisualStyleBackColor = false;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDown.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnDown.FlatAppearance.BorderSize = 2;
            this.btnDown.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDown.Location = new System.Drawing.Point(450, 115);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(60, 50);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "DOWN";
            this.btnDown.UseVisualStyleBackColor = false;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeft.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnLeft.FlatAppearance.BorderSize = 2;
            this.btnLeft.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLeft.Location = new System.Drawing.Point(385, 115);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(60, 50);
            this.btnLeft.TabIndex = 7;
            this.btnLeft.Text = "LEFT";
            this.btnLeft.UseVisualStyleBackColor = false;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnRight
            // 
            this.btnRight.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRight.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnRight.FlatAppearance.BorderSize = 2;
            this.btnRight.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRight.Location = new System.Drawing.Point(515, 115);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(60, 50);
            this.btnRight.TabIndex = 8;
            this.btnRight.Text = "RIGHT";
            this.btnRight.UseVisualStyleBackColor = false;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 330);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(580, 120);
            this.txtLog.TabIndex = 2;
            // 
            // lblControl
            // 
            this.lblControl.AutoSize = true;
            this.lblControl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblControl.Location = new System.Drawing.Point(220, 20);
            this.lblControl.Name = "lblControl";
            this.lblControl.Size = new System.Drawing.Size(160, 21);
            this.lblControl.TabIndex = 3;
            this.lblControl.Text = "Telescope Control";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 308);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(88, 15);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Command Log:";
            // 
            // lblPortInfo
            // 
            this.lblPortInfo.AutoSize = true;
            this.lblPortInfo.ForeColor = System.Drawing.Color.Green;
            this.lblPortInfo.Location = new System.Drawing.Point(400, 20);
            this.lblPortInfo.Name = "lblPortInfo";
            this.lblPortInfo.Size = new System.Drawing.Size(100, 15);
            this.lblPortInfo.TabIndex = 5;
            this.lblPortInfo.Text = "Connected: COM1";
            // 
            // grpTimeBetweenSteps
            // 
            this.grpTimeBetweenSteps.Controls.Add(this.lblStepsPerSecondValue);
            this.grpTimeBetweenSteps.Controls.Add(this.lblTimeValue);
            this.grpTimeBetweenSteps.Controls.Add(this.trackBarStepsPerSecond);
            this.grpTimeBetweenSteps.Controls.Add(this.lblStepsPerSecond);
            this.grpTimeBetweenSteps.Location = new System.Drawing.Point(12, 55);
            this.grpTimeBetweenSteps.Name = "grpTimeBetweenSteps";
            this.grpTimeBetweenSteps.Size = new System.Drawing.Size(360, 145);
            this.grpTimeBetweenSteps.TabIndex = 6;
            this.grpTimeBetweenSteps.TabStop = false;
            this.grpTimeBetweenSteps.Text = "Speed Control";
            // 
            // lblStepsPerSecondValue
            // 
            this.lblStepsPerSecondValue.AutoSize = true;
            this.lblStepsPerSecondValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStepsPerSecondValue.Location = new System.Drawing.Point(15, 105);
            this.lblStepsPerSecondValue.Name = "lblStepsPerSecondValue";
            this.lblStepsPerSecondValue.Size = new System.Drawing.Size(140, 19);
            this.lblStepsPerSecondValue.TabIndex = 3;
            this.lblStepsPerSecondValue.Text = "100 steps/second";
            // 
            // lblTimeValue
            // 
            this.lblTimeValue.AutoSize = true;
            this.lblTimeValue.ForeColor = System.Drawing.Color.Gray;
            this.lblTimeValue.Location = new System.Drawing.Point(180, 107);
            this.lblTimeValue.Name = "lblTimeValue";
            this.lblTimeValue.Size = new System.Drawing.Size(50, 15);
            this.lblTimeValue.TabIndex = 2;
            this.lblTimeValue.Text = "(t=10 ms)";
            // 
            // trackBarStepsPerSecond
            // 
            this.trackBarStepsPerSecond.LargeChange = 1;
            this.trackBarStepsPerSecond.Location = new System.Drawing.Point(15, 50);
            this.trackBarStepsPerSecond.Maximum = 5;
            this.trackBarStepsPerSecond.Minimum = 0;
            this.trackBarStepsPerSecond.Name = "trackBarStepsPerSecond";
            this.trackBarStepsPerSecond.Size = new System.Drawing.Size(330, 45);
            this.trackBarStepsPerSecond.TabIndex = 1;
            this.trackBarStepsPerSecond.Value = 4;
            this.trackBarStepsPerSecond.Scroll += new System.EventHandler(this.trackBarStepsPerSecond_Scroll);
            // 
            // lblStepsPerSecond
            // 
            this.lblStepsPerSecond.AutoSize = true;
            this.lblStepsPerSecond.Location = new System.Drawing.Point(15, 25);
            this.lblStepsPerSecond.Name = "lblStepsPerSecond";
            this.lblStepsPerSecond.Size = new System.Drawing.Size(105, 15);
            this.lblStepsPerSecond.TabIndex = 0;
            this.lblStepsPerSecond.Text = "Steps Per Second:";
            // 
            // grpFocusControl
            // 
            this.grpFocusControl.Controls.Add(this.lblFocusSpeedValue);
            this.grpFocusControl.Controls.Add(this.trackBarFocusSpeed);
            this.grpFocusControl.Controls.Add(this.lblFocusSpeed);
            this.grpFocusControl.Controls.Add(this.btnFocusDecrease);
            this.grpFocusControl.Controls.Add(this.btnFocusIncrease);
            this.grpFocusControl.Location = new System.Drawing.Point(12, 210);
            this.grpFocusControl.Name = "grpFocusControl";
            this.grpFocusControl.Size = new System.Drawing.Size(370, 85);
            this.grpFocusControl.TabIndex = 9;
            this.grpFocusControl.TabStop = false;
            this.grpFocusControl.Text = "Focus Control";
            // 
            // lblFocusSpeedValue
            // 
            this.lblFocusSpeedValue.AutoSize = true;
            this.lblFocusSpeedValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFocusSpeedValue.Location = new System.Drawing.Point(300, 50);
            this.lblFocusSpeedValue.Name = "lblFocusSpeedValue";
            this.lblFocusSpeedValue.Size = new System.Drawing.Size(61, 19);
            this.lblFocusSpeedValue.TabIndex = 4;
            this.lblFocusSpeedValue.Text = "Speed: 9";
            // 
            // trackBarFocusSpeed
            // 
            this.trackBarFocusSpeed.LargeChange = 1;
            this.trackBarFocusSpeed.Location = new System.Drawing.Point(230, 25);
            this.trackBarFocusSpeed.Maximum = 18;
            this.trackBarFocusSpeed.Minimum = 1;
            this.trackBarFocusSpeed.Name = "trackBarFocusSpeed";
            this.trackBarFocusSpeed.Size = new System.Drawing.Size(130, 45);
            this.trackBarFocusSpeed.TabIndex = 3;
            this.trackBarFocusSpeed.Value = 9;
            this.trackBarFocusSpeed.Scroll += new System.EventHandler(this.trackBarFocusSpeed_Scroll);
            // 
            // lblFocusSpeed
            // 
            this.lblFocusSpeed.AutoSize = true;
            this.lblFocusSpeed.Location = new System.Drawing.Point(230, 10);
            this.lblFocusSpeed.Name = "lblFocusSpeed";
            this.lblFocusSpeed.Size = new System.Drawing.Size(76, 15);
            this.lblFocusSpeed.TabIndex = 2;
            this.lblFocusSpeed.Text = "Motor Speed:";
            // 
            // btnFocusDecrease
            // 
            this.btnFocusDecrease.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFocusDecrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFocusDecrease.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnFocusDecrease.FlatAppearance.BorderSize = 2;
            this.btnFocusDecrease.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnFocusDecrease.Location = new System.Drawing.Point(115, 25);
            this.btnFocusDecrease.Name = "btnFocusDecrease";
            this.btnFocusDecrease.Size = new System.Drawing.Size(100, 40);
            this.btnFocusDecrease.TabIndex = 1;
            this.btnFocusDecrease.Text = "NEAR (-)";
            this.btnFocusDecrease.UseVisualStyleBackColor = false;
            this.btnFocusDecrease.Click += new System.EventHandler(this.btnFocusDecrease_Click);
            // 
            // btnFocusIncrease
            // 
            this.btnFocusIncrease.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFocusIncrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFocusIncrease.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnFocusIncrease.FlatAppearance.BorderSize = 2;
            this.btnFocusIncrease.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnFocusIncrease.Location = new System.Drawing.Point(15, 25);
            this.btnFocusIncrease.Name = "btnFocusIncrease";
            this.btnFocusIncrease.Size = new System.Drawing.Size(90, 40);
            this.btnFocusIncrease.TabIndex = 0;
            this.btnFocusIncrease.Text = "FAR (+)";
            this.btnFocusIncrease.UseVisualStyleBackColor = false;
            this.btnFocusIncrease.Click += new System.EventHandler(this.btnFocusIncrease_Click);
            // 
            // grpVideoStream
            // 
            this.grpVideoStream.Controls.Add(this.lblVideoStatus);
            this.grpVideoStream.Controls.Add(this.btnVideoStop);
            this.grpVideoStream.Controls.Add(this.btnVideoStart);
            this.grpVideoStream.Location = new System.Drawing.Point(390, 210);
            this.grpVideoStream.Name = "grpVideoStream";
            this.grpVideoStream.Size = new System.Drawing.Size(200, 85);
            this.grpVideoStream.TabIndex = 10;
            this.grpVideoStream.TabStop = false;
            this.grpVideoStream.Text = "Video Stream";
            // 
            // lblVideoStatus
            // 
            this.lblVideoStatus.AutoSize = true;
            this.lblVideoStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblVideoStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblVideoStatus.Location = new System.Drawing.Point(60, 60);
            this.lblVideoStatus.Name = "lblVideoStatus";
            this.lblVideoStatus.Size = new System.Drawing.Size(80, 15);
            this.lblVideoStatus.TabIndex = 2;
            this.lblVideoStatus.Text = "Checking...";
            // 
            // btnVideoStop
            // 
            this.btnVideoStop.BackColor = System.Drawing.Color.MistyRose;
            this.btnVideoStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVideoStop.FlatAppearance.BorderColor = System.Drawing.Color.IndianRed;
            this.btnVideoStop.FlatAppearance.BorderSize = 2;
            this.btnVideoStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnVideoStop.Location = new System.Drawing.Point(105, 20);
            this.btnVideoStop.Name = "btnVideoStop";
            this.btnVideoStop.Size = new System.Drawing.Size(80, 35);
            this.btnVideoStop.TabIndex = 1;
            this.btnVideoStop.Text = "Stop";
            this.btnVideoStop.UseVisualStyleBackColor = false;
            this.btnVideoStop.Click += new System.EventHandler(this.btnVideoStop_Click);
            // 
            // btnVideoStart
            // 
            this.btnVideoStart.BackColor = System.Drawing.Color.Honeydew;
            this.btnVideoStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVideoStart.FlatAppearance.BorderColor = System.Drawing.Color.Green;
            this.btnVideoStart.FlatAppearance.BorderSize = 2;
            this.btnVideoStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnVideoStart.Location = new System.Drawing.Point(15, 20);
            this.btnVideoStart.Name = "btnVideoStart";
            this.btnVideoStart.Size = new System.Drawing.Size(80, 35);
            this.btnVideoStart.TabIndex = 0;
            this.btnVideoStart.Text = "Start";
            this.btnVideoStart.UseVisualStyleBackColor = false;
            this.btnVideoStart.Click += new System.EventHandler(this.btnVideoStart_Click);
            // 
            // TelescopeControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 465);
            this.Controls.Add(this.grpVideoStream);
            this.Controls.Add(this.grpFocusControl);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.grpTimeBetweenSteps);
            this.Controls.Add(this.lblPortInfo);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblControl);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "TelescopeControlForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Telescope Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TelescopeControlForm_FormClosing);
            this.grpTimeBetweenSteps.ResumeLayout(false);
            this.grpTimeBetweenSteps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).EndInit();
            this.grpFocusControl.ResumeLayout(false);
            this.grpFocusControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).EndInit();
            this.grpVideoStream.ResumeLayout(false);
            this.grpVideoStream.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblControl;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPortInfo;
        private System.Windows.Forms.GroupBox grpTimeBetweenSteps;
        private System.Windows.Forms.TrackBar trackBarStepsPerSecond;
        private System.Windows.Forms.Label lblStepsPerSecond;
        private System.Windows.Forms.Label lblTimeValue;
        private System.Windows.Forms.Label lblStepsPerSecondValue;
        private System.Windows.Forms.GroupBox grpFocusControl;
        private System.Windows.Forms.Button btnFocusIncrease;
        private System.Windows.Forms.Button btnFocusDecrease;
        private System.Windows.Forms.Label lblFocusSpeed;
        private System.Windows.Forms.TrackBar trackBarFocusSpeed;
        private System.Windows.Forms.Label lblFocusSpeedValue;
        private System.Windows.Forms.GroupBox grpVideoStream;
        private System.Windows.Forms.Button btnVideoStart;
        private System.Windows.Forms.Button btnVideoStop;
        private System.Windows.Forms.Label lblVideoStatus;
    }
}
