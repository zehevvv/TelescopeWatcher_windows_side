namespace TelescopeWatcher
{
    partial class PlateSolverForm
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
            this.lblCamera = new System.Windows.Forms.Label();
            this.comboCamera = new System.Windows.Forms.ComboBox();
            this.btnSolve = new System.Windows.Forms.Button();
            this.lblRa = new System.Windows.Forms.Label();
            this.txtRa = new System.Windows.Forms.TextBox();
            this.lblDec = new System.Windows.Forms.Label();
            this.txtDec = new System.Windows.Forms.TextBox();
            this.lblRotation = new System.Windows.Forms.Label();
            this.txtRotation = new System.Windows.Forms.TextBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblCamera
            // 
            this.lblCamera.AutoSize = true;
            this.lblCamera.Location = new System.Drawing.Point(12, 15);
            this.lblCamera.Name = "lblCamera";
            this.lblCamera.Size = new System.Drawing.Size(51, 15);
            this.lblCamera.TabIndex = 0;
            this.lblCamera.Text = "Camera:";
            // 
            // comboCamera
            // 
            this.comboCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCamera.FormattingEnabled = true;
            this.comboCamera.Items.AddRange(new object[] {
            "hd",
            "uc60"});
            this.comboCamera.Location = new System.Drawing.Point(69, 12);
            this.comboCamera.Name = "comboCamera";
            this.comboCamera.Size = new System.Drawing.Size(121, 23);
            this.comboCamera.TabIndex = 1;
            // 
            // btnSolve
            // 
            this.btnSolve.Location = new System.Drawing.Point(206, 11);
            this.btnSolve.Name = "btnSolve";
            this.btnSolve.Size = new System.Drawing.Size(85, 25);
            this.btnSolve.TabIndex = 2;
            this.btnSolve.Text = "Solve Plate";
            this.btnSolve.UseVisualStyleBackColor = true;
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
            // 
            // lblRa
            // 
            this.lblRa.AutoSize = true;
            this.lblRa.Location = new System.Drawing.Point(12, 55);
            this.lblRa.Name = "lblRa";
            this.lblRa.Size = new System.Drawing.Size(25, 15);
            this.lblRa.TabIndex = 3;
            this.lblRa.Text = "RA:";
            // 
            // txtRa
            // 
            this.txtRa.Location = new System.Drawing.Point(69, 52);
            this.txtRa.Name = "txtRa";
            this.txtRa.ReadOnly = true;
            this.txtRa.Size = new System.Drawing.Size(121, 23);
            this.txtRa.TabIndex = 4;
            // 
            // lblDec
            // 
            this.lblDec.AutoSize = true;
            this.lblDec.Location = new System.Drawing.Point(12, 84);
            this.lblDec.Name = "lblDec";
            this.lblDec.Size = new System.Drawing.Size(32, 15);
            this.lblDec.TabIndex = 5;
            this.lblDec.Text = "DEC:";
            // 
            // txtDec
            // 
            this.txtDec.Location = new System.Drawing.Point(69, 81);
            this.txtDec.Name = "txtDec";
            this.txtDec.ReadOnly = true;
            this.txtDec.Size = new System.Drawing.Size(121, 23);
            this.txtDec.TabIndex = 6;
            // 
            // lblRotation
            // 
            this.lblRotation.AutoSize = true;
            this.lblRotation.Location = new System.Drawing.Point(203, 55);
            this.lblRotation.Name = "lblRotation";
            this.lblRotation.Size = new System.Drawing.Size(55, 15);
            this.lblRotation.TabIndex = 7;
            this.lblRotation.Text = "Rotation:";
            // 
            // txtRotation
            // 
            this.txtRotation.Location = new System.Drawing.Point(264, 52);
            this.txtRotation.Name = "txtRotation";
            this.txtRotation.ReadOnly = true;
            this.txtRotation.Size = new System.Drawing.Size(121, 23);
            this.txtRotation.TabIndex = 8;
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.Location = new System.Drawing.Point(12, 120);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(390, 150);
            this.txtStatus.TabIndex = 9;
            // 
            // PlateSolverForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 281);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.txtRotation);
            this.Controls.Add(this.lblRotation);
            this.Controls.Add(this.txtDec);
            this.Controls.Add(this.lblDec);
            this.Controls.Add(this.txtRa);
            this.Controls.Add(this.lblRa);
            this.Controls.Add(this.btnSolve);
            this.Controls.Add(this.comboCamera);
            this.Controls.Add(this.lblCamera);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PlateSolverForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plate Solver";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblCamera;
        private System.Windows.Forms.ComboBox comboCamera;
        private System.Windows.Forms.Button btnSolve;
        private System.Windows.Forms.Label lblRa;
        private System.Windows.Forms.TextBox txtRa;
        private System.Windows.Forms.Label lblDec;
        private System.Windows.Forms.TextBox txtDec;
        private System.Windows.Forms.Label lblRotation;
        private System.Windows.Forms.TextBox txtRotation;
        private System.Windows.Forms.TextBox txtStatus;
    }
}