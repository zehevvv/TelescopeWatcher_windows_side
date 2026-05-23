namespace TelescopeWatcher
{
    partial class StackingForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblFps = new System.Windows.Forms.Label();
            this.radioTimeBased = new System.Windows.Forms.RadioButton();
            this.radioFrameBased = new System.Windows.Forms.RadioButton();

            // Time-based controls
            this.lblSecondsLabel = new System.Windows.Forms.Label();
            this.numSeconds = new System.Windows.Forms.NumericUpDown();
            this.lblFramesEstimate = new System.Windows.Forms.Label();

            // Frame-based controls
            this.lblFramesLabel = new System.Windows.Forms.Label();
            this.numFrames = new System.Windows.Forms.NumericUpDown();
            this.lblTimeEstimate = new System.Windows.Forms.Label();

            // Buttons & progress
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblProgress = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).BeginInit();
            this.SuspendLayout();

            // Form
            this.Text = "Frame Stacking";
            this.Size = new System.Drawing.Size(420, 380);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;

            // lblFps
            this.lblFps.Location = new System.Drawing.Point(12, 12);
            this.lblFps.Size = new System.Drawing.Size(380, 20);
            this.lblFps.Text = "Main camera FPS: ...";
            this.lblFps.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

            // radioTimeBased
            this.radioTimeBased.Location = new System.Drawing.Point(12, 44);
            this.radioTimeBased.Size = new System.Drawing.Size(200, 22);
            this.radioTimeBased.Text = "Stacking for limited time";
            this.radioTimeBased.Checked = true;
            this.radioTimeBased.ForeColor = System.Drawing.Color.White;

            // lblSecondsLabel
            this.lblSecondsLabel.Location = new System.Drawing.Point(30, 70);
            this.lblSecondsLabel.Size = new System.Drawing.Size(120, 20);
            this.lblSecondsLabel.Text = "Duration (seconds):";

            // numSeconds
            this.numSeconds.Location = new System.Drawing.Point(160, 68);
            this.numSeconds.Size = new System.Drawing.Size(80, 22);
            this.numSeconds.Minimum = 1;
            this.numSeconds.Maximum = 3600;
            this.numSeconds.Value = 10;

            // lblFramesEstimate
            this.lblFramesEstimate.Location = new System.Drawing.Point(30, 95);
            this.lblFramesEstimate.Size = new System.Drawing.Size(350, 20);
            this.lblFramesEstimate.ForeColor = System.Drawing.Color.LightGray;
            this.lblFramesEstimate.Text = "";

            // radioFrameBased
            this.radioFrameBased.Location = new System.Drawing.Point(12, 128);
            this.radioFrameBased.Size = new System.Drawing.Size(200, 22);
            this.radioFrameBased.Text = "Stacking by frame number";
            this.radioFrameBased.ForeColor = System.Drawing.Color.White;

            // lblFramesLabel
            this.lblFramesLabel.Location = new System.Drawing.Point(30, 154);
            this.lblFramesLabel.Size = new System.Drawing.Size(120, 20);
            this.lblFramesLabel.Text = "Number of frames:";

            // numFrames
            this.numFrames.Location = new System.Drawing.Point(160, 152);
            this.numFrames.Size = new System.Drawing.Size(80, 22);
            this.numFrames.Minimum = 1;
            this.numFrames.Maximum = 100000;
            this.numFrames.Value = 100;

            // lblTimeEstimate
            this.lblTimeEstimate.Location = new System.Drawing.Point(30, 179);
            this.lblTimeEstimate.Size = new System.Drawing.Size(350, 20);
            this.lblTimeEstimate.ForeColor = System.Drawing.Color.LightGray;
            this.lblTimeEstimate.Text = "";

            // btnStart
            this.btnStart.Location = new System.Drawing.Point(12, 215);
            this.btnStart.Size = new System.Drawing.Size(100, 32);
            this.btnStart.Text = "Start";
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(0, 120, 0);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);

            // btnStop
            this.btnStop.Location = new System.Drawing.Point(120, 215);
            this.btnStop.Size = new System.Drawing.Size(100, 32);
            this.btnStop.Text = "Stop";
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(160, 0, 0);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Enabled = false;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);

            // lblProgress
            this.lblProgress.Location = new System.Drawing.Point(12, 258);
            this.lblProgress.Size = new System.Drawing.Size(380, 40);
            this.lblProgress.ForeColor = System.Drawing.Color.LightYellow;
            this.lblProgress.Text = "";
            this.lblProgress.Visible = true;

            // Add controls
            this.Controls.Add(this.lblFps);
            this.Controls.Add(this.radioTimeBased);
            this.Controls.Add(this.lblSecondsLabel);
            this.Controls.Add(this.numSeconds);
            this.Controls.Add(this.lblFramesEstimate);
            this.Controls.Add(this.radioFrameBased);
            this.Controls.Add(this.lblFramesLabel);
            this.Controls.Add(this.numFrames);
            this.Controls.Add(this.lblTimeEstimate);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.lblProgress);

            ((System.ComponentModel.ISupportInitialize)(this.numSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFrames)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.RadioButton radioTimeBased;
        private System.Windows.Forms.RadioButton radioFrameBased;
        private System.Windows.Forms.Label lblSecondsLabel;
        private System.Windows.Forms.NumericUpDown numSeconds;
        private System.Windows.Forms.Label lblFramesEstimate;
        private System.Windows.Forms.Label lblFramesLabel;
        private System.Windows.Forms.NumericUpDown numFrames;
        private System.Windows.Forms.Label lblTimeEstimate;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblProgress;
    }
}
