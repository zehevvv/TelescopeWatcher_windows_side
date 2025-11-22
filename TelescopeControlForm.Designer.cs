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
            this.grpTimeBetweenSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUp
            // 
            this.btnUp.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnUp.Location = new System.Drawing.Point(390, 60);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(80, 60);
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "? UP";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnDown.Location = new System.Drawing.Point(390, 130);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(80, 60);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "? DOWN";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnLeft.Location = new System.Drawing.Point(500, 60);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(80, 60);
            this.btnLeft.TabIndex = 7;
            this.btnLeft.Text = "? LEFT";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnRight
            // 
            this.btnRight.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnRight.Location = new System.Drawing.Point(500, 130);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(80, 60);
            this.btnRight.TabIndex = 8;
            this.btnRight.Text = "? RIGHT";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 250);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(580, 150);
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
            this.lblStatus.Location = new System.Drawing.Point(12, 230);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(88, 15);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Command Log:";
            // 
            // lblPortInfo
            // 
            this.lblPortInfo.AutoSize = true;
            this.lblPortInfo.ForeColor = System.Drawing.Color.Green;
            this.lblPortInfo.Location = new System.Drawing.Point(12, 210);
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
            this.trackBarStepsPerSecond.Value = 2;
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
            // TelescopeControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 411);
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
    }
}
