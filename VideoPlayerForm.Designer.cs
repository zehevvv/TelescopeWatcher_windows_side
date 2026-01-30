namespace TelescopeWatcher
{
    partial class VideoPlayerForm
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
            this.videoView1 = new LibVLCSharp.WinForms.VideoView();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFrameInfo1 = new System.Windows.Forms.Label();
            this.lblFrameInfo2 = new System.Windows.Forms.Label();
            this.chkFlipHorizontal = new System.Windows.Forms.CheckBox();
            this.chkFlipVertical = new System.Windows.Forms.CheckBox();
            this.radioMainOnly = new System.Windows.Forms.RadioButton();
            this.radioSecondaryOnly = new System.Windows.Forms.RadioButton();
            this.radioBoth = new System.Windows.Forms.RadioButton();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.btnAddCircle = new System.Windows.Forms.Button();
            this.btnCircleSizeIncrease = new System.Windows.Forms.Button();
            this.btnCircleSizeDecrease = new System.Windows.Forms.Button();
            this.lblCircleSize = new System.Windows.Forms.Label();
            this.btnMainCameraControl = new System.Windows.Forms.Button();
            this.btnSecondaryCameraControl = new System.Windows.Forms.Button();
            this.telescopeControlPanel = new System.Windows.Forms.Panel();
            this.trackBarStepsPerSecond = new System.Windows.Forms.TrackBar();
            this.lblStepsPerSecond = new System.Windows.Forms.Label();
            this.lblStepsPerSecondValue = new System.Windows.Forms.Label();
            this.trackBarFocusSpeed = new System.Windows.Forms.TrackBar();
            this.lblFocusSpeed = new System.Windows.Forms.Label();
            this.lblFocusSpeedValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.videoPanel.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.telescopeControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // videoView1
            // 
            this.videoView1.BackColor = System.Drawing.Color.Black;
            this.videoView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoView1.MediaPlayer = null;
            this.videoView1.Name = "videoView1";
            this.videoView1.TabIndex = 0;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.Black;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox2_Paint);
            this.pictureBox2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PictureBox2_MouseClick);
            this.pictureBox2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox2_MouseMove);
            // 
            // videoPanel
            // 
            this.videoPanel.BackColor = System.Drawing.Color.Black;
            this.videoPanel.Controls.Add(this.videoView1);
            this.videoPanel.Controls.Add(this.pictureBox2);
            this.videoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.TabIndex = 2;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.IndianRed;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Height = 40;
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "Close Stream";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Black;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Height = 30;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Connecting to streams...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFrameInfo1
            // 
            this.lblFrameInfo1.BackColor = System.Drawing.Color.Black;
            this.lblFrameInfo1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblFrameInfo1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFrameInfo1.ForeColor = System.Drawing.Color.LightGray;
            this.lblFrameInfo1.Height = 25;
            this.lblFrameInfo1.Name = "lblFrameInfo1";
            this.lblFrameInfo1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblFrameInfo1.Text = "Main: RTSP Stream";
            this.lblFrameInfo1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFrameInfo2
            // 
            this.lblFrameInfo2.BackColor = System.Drawing.Color.Black;
            this.lblFrameInfo2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblFrameInfo2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFrameInfo2.ForeColor = System.Drawing.Color.LightGray;
            this.lblFrameInfo2.Height = 25;
            this.lblFrameInfo2.Name = "lblFrameInfo2";
            this.lblFrameInfo2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblFrameInfo2.Text = "Secondary: Frame 0 | FPS: 0.0";
            this.lblFrameInfo2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkFlipHorizontal
            // 
            this.chkFlipHorizontal.AutoSize = true;
            this.chkFlipHorizontal.Checked = true;
            this.chkFlipHorizontal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFlipHorizontal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkFlipHorizontal.ForeColor = System.Drawing.Color.White;
            this.chkFlipHorizontal.Location = new System.Drawing.Point(420, 8);
            this.chkFlipHorizontal.Name = "chkFlipHorizontal";
            this.chkFlipHorizontal.Text = "Flip H";
            this.chkFlipHorizontal.UseVisualStyleBackColor = true;
            this.chkFlipHorizontal.CheckedChanged += new System.EventHandler(this.ChkFlipHorizontal_CheckedChanged);
            // 
            // chkFlipVertical
            // 
            this.chkFlipVertical.AutoSize = true;
            this.chkFlipVertical.Checked = true;
            this.chkFlipVertical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFlipVertical.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkFlipVertical.ForeColor = System.Drawing.Color.White;
            this.chkFlipVertical.Location = new System.Drawing.Point(500, 8);
            this.chkFlipVertical.Name = "chkFlipVertical";
            this.chkFlipVertical.Text = "Flip V";
            this.chkFlipVertical.UseVisualStyleBackColor = true;
            this.chkFlipVertical.CheckedChanged += new System.EventHandler(this.ChkFlipVertical_CheckedChanged);
            // 
            // radioMainOnly
            // 
            this.radioMainOnly.AutoSize = true;
            this.radioMainOnly.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radioMainOnly.ForeColor = System.Drawing.Color.White;
            this.radioMainOnly.Location = new System.Drawing.Point(10, 8);
            this.radioMainOnly.Name = "radioMainOnly";
            this.radioMainOnly.Text = "Main Camera";
            this.radioMainOnly.UseVisualStyleBackColor = true;
            this.radioMainOnly.CheckedChanged += new System.EventHandler(this.RadioStream_CheckedChanged);
            // 
            // radioSecondaryOnly
            // 
            this.radioSecondaryOnly.AutoSize = true;
            this.radioSecondaryOnly.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radioSecondaryOnly.ForeColor = System.Drawing.Color.White;
            this.radioSecondaryOnly.Location = new System.Drawing.Point(130, 8);
            this.radioSecondaryOnly.Name = "radioSecondaryOnly";
            this.radioSecondaryOnly.Text = "Secondary Camera";
            this.radioSecondaryOnly.UseVisualStyleBackColor = true;
            this.radioSecondaryOnly.CheckedChanged += new System.EventHandler(this.RadioStream_CheckedChanged);
            // 
            // radioBoth
            // 
            this.radioBoth.AutoSize = true;
            this.radioBoth.Checked = true;
            this.radioBoth.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radioBoth.ForeColor = System.Drawing.Color.White;
            this.radioBoth.Location = new System.Drawing.Point(280, 8);
            this.radioBoth.Name = "radioBoth";
            this.radioBoth.TabStop = true;
            this.radioBoth.Text = "Both Cameras";
            this.radioBoth.UseVisualStyleBackColor = true;
            this.radioBoth.CheckedChanged += new System.EventHandler(this.RadioStream_CheckedChanged);
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.controlPanel.Controls.Add(this.radioMainOnly);
            this.controlPanel.Controls.Add(this.radioSecondaryOnly);
            this.controlPanel.Controls.Add(this.radioBoth);
            this.controlPanel.Controls.Add(this.chkFlipHorizontal);
            this.controlPanel.Controls.Add(this.chkFlipVertical);
            this.controlPanel.Controls.Add(this.btnMainCameraControl);
            this.controlPanel.Controls.Add(this.btnSecondaryCameraControl);
            this.controlPanel.Controls.Add(this.btnAddCircle);
            this.controlPanel.Controls.Add(this.btnCircleSizeDecrease);
            this.controlPanel.Controls.Add(this.lblCircleSize);
            this.controlPanel.Controls.Add(this.btnCircleSizeIncrease);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.controlPanel.Height = 35;
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.controlPanel.Resize += new System.EventHandler((s, e) => PositionCircleControls());
            // 
            // btnAddCircle
            // 
            this.btnAddCircle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCircle.AutoSize = true;
            this.btnAddCircle.BackColor = System.Drawing.Color.DarkRed;
            this.btnAddCircle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddCircle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddCircle.ForeColor = System.Drawing.Color.White;
            this.btnAddCircle.Name = "btnAddCircle";
            this.btnAddCircle.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnAddCircle.Text = "Add Circle";
            this.btnAddCircle.UseVisualStyleBackColor = false;
            this.btnAddCircle.Click += new System.EventHandler(this.BtnAddCircle_Click);
            // 
            // btnCircleSizeIncrease
            // 
            this.btnCircleSizeIncrease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCircleSizeIncrease.BackColor = System.Drawing.Color.DarkSlateGray;
            this.btnCircleSizeIncrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCircleSizeIncrease.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCircleSizeIncrease.ForeColor = System.Drawing.Color.White;
            this.btnCircleSizeIncrease.Height = 28;
            this.btnCircleSizeIncrease.Name = "btnCircleSizeIncrease";
            this.btnCircleSizeIncrease.Text = "+";
            this.btnCircleSizeIncrease.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCircleSizeIncrease.UseVisualStyleBackColor = false;
            this.btnCircleSizeIncrease.Width = 30;
            this.btnCircleSizeIncrease.Click += new System.EventHandler(this.BtnCircleSizeIncrease_Click);
            // 
            // btnCircleSizeDecrease
            // 
            this.btnCircleSizeDecrease.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCircleSizeDecrease.BackColor = System.Drawing.Color.DarkSlateGray;
            this.btnCircleSizeDecrease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCircleSizeDecrease.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCircleSizeDecrease.ForeColor = System.Drawing.Color.White;
            this.btnCircleSizeDecrease.Height = 28;
            this.btnCircleSizeDecrease.Name = "btnCircleSizeDecrease";
            this.btnCircleSizeDecrease.Text = "-";
            this.btnCircleSizeDecrease.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCircleSizeDecrease.UseVisualStyleBackColor = false;
            this.btnCircleSizeDecrease.Width = 30;
            this.btnCircleSizeDecrease.Click += new System.EventHandler(this.BtnCircleSizeDecrease_Click);
            // 
            // lblCircleSize
            // 
            this.lblCircleSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCircleSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCircleSize.ForeColor = System.Drawing.Color.White;
            this.lblCircleSize.Height = 20;
            this.lblCircleSize.Name = "lblCircleSize";
            this.lblCircleSize.Text = "30";
            this.lblCircleSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCircleSize.Width = 40;
            // 
            // btnMainCameraControl
            // 
            this.btnMainCameraControl.AutoSize = true;
            this.btnMainCameraControl.BackColor = System.Drawing.Color.DarkBlue;
            this.btnMainCameraControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMainCameraControl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMainCameraControl.ForeColor = System.Drawing.Color.White;
            this.btnMainCameraControl.Location = new System.Drawing.Point(580, 5);
            this.btnMainCameraControl.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnMainCameraControl.Name = "btnMainCameraControl";
            this.btnMainCameraControl.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.btnMainCameraControl.Text = "Main Cam Control";
            this.btnMainCameraControl.UseVisualStyleBackColor = false;
            this.btnMainCameraControl.Click += new System.EventHandler(this.BtnMainCameraControl_Click);
            // 
            // btnSecondaryCameraControl
            // 
            this.btnSecondaryCameraControl.AutoSize = true;
            this.btnSecondaryCameraControl.BackColor = System.Drawing.Color.DarkBlue;
            this.btnSecondaryCameraControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSecondaryCameraControl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSecondaryCameraControl.ForeColor = System.Drawing.Color.White;
            this.btnSecondaryCameraControl.Location = new System.Drawing.Point(730, 5);
            this.btnSecondaryCameraControl.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnSecondaryCameraControl.Name = "btnSecondaryCameraControl";
            this.btnSecondaryCameraControl.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.btnSecondaryCameraControl.Text = "Sec Cam Control";
            this.btnSecondaryCameraControl.UseVisualStyleBackColor = false;
            this.btnSecondaryCameraControl.Click += new System.EventHandler(this.BtnSecondaryCameraControl_Click);
            // 
            // telescopeControlPanel
            // 
            this.telescopeControlPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.telescopeControlPanel.Controls.Add(this.lblStepsPerSecond);
            this.telescopeControlPanel.Controls.Add(this.trackBarStepsPerSecond);
            this.telescopeControlPanel.Controls.Add(this.lblStepsPerSecondValue);
            this.telescopeControlPanel.Controls.Add(this.lblFocusSpeed);
            this.telescopeControlPanel.Controls.Add(this.trackBarFocusSpeed);
            this.telescopeControlPanel.Controls.Add(this.lblFocusSpeedValue);
            this.telescopeControlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.telescopeControlPanel.Height = 80;
            this.telescopeControlPanel.Name = "telescopeControlPanel";
            this.telescopeControlPanel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.telescopeControlPanel.Resize += new System.EventHandler((s, e) => PositionFocusControls());
            // 
            // trackBarStepsPerSecond
            // 
            this.trackBarStepsPerSecond.LargeChange = 1;
            this.trackBarStepsPerSecond.Location = new System.Drawing.Point(10, 30);
            this.trackBarStepsPerSecond.Maximum = 4;
            this.trackBarStepsPerSecond.Name = "trackBarStepsPerSecond";
            this.trackBarStepsPerSecond.Size = new System.Drawing.Size(300, 45);
            this.trackBarStepsPerSecond.TabIndex = 0;
            this.trackBarStepsPerSecond.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarStepsPerSecond.ValueChanged += new System.EventHandler(this.TrackBarStepsPerSecond_ValueChanged);
            // 
            // lblStepsPerSecond
            // 
            this.lblStepsPerSecond.AutoSize = true;
            this.lblStepsPerSecond.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStepsPerSecond.ForeColor = System.Drawing.Color.White;
            this.lblStepsPerSecond.Location = new System.Drawing.Point(10, 10);
            this.lblStepsPerSecond.Name = "lblStepsPerSecond";
            this.lblStepsPerSecond.Text = "Steps/Second:";
            // 
            // lblStepsPerSecondValue
            // 
            this.lblStepsPerSecondValue.AutoSize = true;
            this.lblStepsPerSecondValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStepsPerSecondValue.ForeColor = System.Drawing.Color.LightGreen;
            this.lblStepsPerSecondValue.Location = new System.Drawing.Point(320, 35);
            this.lblStepsPerSecondValue.Name = "lblStepsPerSecondValue";
            this.lblStepsPerSecondValue.Text = "";
            // 
            // trackBarFocusSpeed
            // 
            this.trackBarFocusSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarFocusSpeed.LargeChange = 2;
            this.trackBarFocusSpeed.Maximum = 18;
            this.trackBarFocusSpeed.Minimum = 1;
            this.trackBarFocusSpeed.Name = "trackBarFocusSpeed";
            this.trackBarFocusSpeed.Size = new System.Drawing.Size(250, 45);
            this.trackBarFocusSpeed.TabIndex = 1;
            this.trackBarFocusSpeed.Value = 9;
            this.trackBarFocusSpeed.ValueChanged += new System.EventHandler(this.TrackBarFocusSpeed_ValueChanged);
            // 
            // lblFocusSpeed
            // 
            this.lblFocusSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFocusSpeed.AutoSize = true;
            this.lblFocusSpeed.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFocusSpeed.ForeColor = System.Drawing.Color.White;
            this.lblFocusSpeed.Name = "lblFocusSpeed";
            this.lblFocusSpeed.Text = "Focus Motor Speed:";
            // 
            // lblFocusSpeedValue
            // 
            this.lblFocusSpeedValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFocusSpeedValue.AutoSize = true;
            this.lblFocusSpeedValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFocusSpeedValue.ForeColor = System.Drawing.Color.LightGreen;
            this.lblFocusSpeedValue.Name = "lblFocusSpeedValue";
            this.lblFocusSpeedValue.Text = "";
            // 
            // VideoPlayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 680);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblFrameInfo1);
            this.Controls.Add(this.lblFrameInfo2);
            this.Controls.Add(this.telescopeControlPanel);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(600, 500);
            this.Name = "VideoPlayerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Video Stream - RTSP/MJPEG";
            this.Load += new System.EventHandler(this.VideoPlayerForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VideoPlayerForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VideoPlayerForm_KeyUp);
            this.Resize += new System.EventHandler(this.VideoPlayerForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.videoView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.videoPanel.ResumeLayout(false);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.telescopeControlPanel.ResumeLayout(false);
            this.telescopeControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private LibVLCSharp.WinForms.VideoView videoView1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFrameInfo1;
        private System.Windows.Forms.Label lblFrameInfo2;
        private System.Windows.Forms.CheckBox chkFlipHorizontal;
        private System.Windows.Forms.CheckBox chkFlipVertical;
        private System.Windows.Forms.RadioButton radioMainOnly;
        private System.Windows.Forms.RadioButton radioSecondaryOnly;
        private System.Windows.Forms.RadioButton radioBoth;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Button btnAddCircle;
        private System.Windows.Forms.Button btnCircleSizeIncrease;
        private System.Windows.Forms.Button btnCircleSizeDecrease;
        private System.Windows.Forms.Label lblCircleSize;
        private System.Windows.Forms.Button btnMainCameraControl;
        private System.Windows.Forms.Button btnSecondaryCameraControl;
        private System.Windows.Forms.Panel telescopeControlPanel;
        private System.Windows.Forms.TrackBar trackBarStepsPerSecond;
        private System.Windows.Forms.Label lblStepsPerSecond;
        private System.Windows.Forms.Label lblStepsPerSecondValue;
        private System.Windows.Forms.TrackBar trackBarFocusSpeed;
        private System.Windows.Forms.Label lblFocusSpeed;
        private System.Windows.Forms.Label lblFocusSpeedValue;
    }
}