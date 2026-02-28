namespace TelescopeWatcher
{
    partial class StarFollowerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbCamera = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSpeedCmd = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtStepsCmd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numThreshold = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numDuration = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStatus = new System.Windows.Forms.Button();
            this.btnDebug = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbCamera);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtSpeedCmd);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtStepsCmd);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numThreshold);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numDuration);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 180);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // cbCamera
            // 
            this.cbCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCamera.FormattingEnabled = true;
            this.cbCamera.Items.AddRange(new object[] {
            "hd",
            "uc60"});
            this.cbCamera.Location = new System.Drawing.Point(100, 140);
            this.cbCamera.Name = "cbCamera";
            this.cbCamera.Size = new System.Drawing.Size(120, 23);
            this.cbCamera.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Camera:";
            // 
            // txtSpeedCmd
            // 
            this.txtSpeedCmd.Location = new System.Drawing.Point(100, 110);
            this.txtSpeedCmd.Name = "txtSpeedCmd";
            this.txtSpeedCmd.Size = new System.Drawing.Size(120, 23);
            this.txtSpeedCmd.TabIndex = 7;
            this.txtSpeedCmd.Text = "s=500";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 113);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Speed Cmd:";
            // 
            // txtStepsCmd
            // 
            this.txtStepsCmd.Location = new System.Drawing.Point(100, 80);
            this.txtStepsCmd.Name = "txtStepsCmd";
            this.txtStepsCmd.Size = new System.Drawing.Size(120, 23);
            this.txtStepsCmd.TabIndex = 5;
            this.txtStepsCmd.Text = "s=500";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Steps Cmd:";
            // 
            // numThreshold
            // 
            this.numThreshold.Location = new System.Drawing.Point(100, 50);
            this.numThreshold.Name = "numThreshold";
            this.numThreshold.Size = new System.Drawing.Size(120, 23);
            this.numThreshold.TabIndex = 3;
            this.numThreshold.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Threshold(%):";
            // 
            // numDuration
            // 
            this.numDuration.DecimalPlaces = 1;
            this.numDuration.Location = new System.Drawing.Point(100, 20);
            this.numDuration.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numDuration.Name = "numDuration";
            this.numDuration.Size = new System.Drawing.Size(120, 23);
            this.numDuration.TabIndex = 1;
            this.numDuration.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Duration(sec):";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(280, 20);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(90, 30);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(280, 60);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(90, 30);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStatus
            // 
            this.btnStatus.Location = new System.Drawing.Point(280, 100);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(90, 30);
            this.btnStatus.TabIndex = 3;
            this.btnStatus.Text = "Status";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.btnStatus_Click);
            // 
            // btnDebug
            // 
            this.btnDebug.Location = new System.Drawing.Point(280, 140);
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(90, 30);
            this.btnDebug.TabIndex = 4;
            this.btnDebug.Text = "Debug Star";
            this.btnDebug.UseVisualStyleBackColor = true;
            this.btnDebug.Click += new System.EventHandler(this.btnDebug_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(12, 198);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(358, 200);
            this.txtOutput.TabIndex = 5;
            // 
            // StarFollowerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 411);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnDebug);
            this.Controls.Add(this.btnStatus);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "StarFollowerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Star Follower";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbCamera;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSpeedCmd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtStepsCmd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numThreshold;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numDuration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.Button btnDebug;
        private System.Windows.Forms.TextBox txtOutput;
    }
}
