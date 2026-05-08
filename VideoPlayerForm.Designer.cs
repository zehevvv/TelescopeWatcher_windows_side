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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.cameraSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.secondaryCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plateSolverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starFollowerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starFollower2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starFollower3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.siderealTrackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFrameInfo1 = new System.Windows.Forms.Label();
            this.lblFrameInfo2 = new System.Windows.Forms.Label();
            this.chkMainFlipH = new System.Windows.Forms.CheckBox();
            this.chkMainFlipV = new System.Windows.Forms.CheckBox();
            this.chkSecFlipH = new System.Windows.Forms.CheckBox();
            this.chkSecFlipV = new System.Windows.Forms.CheckBox();
            this.radioMainOnly = new System.Windows.Forms.RadioButton();
            this.radioSecondaryOnly = new System.Windows.Forms.RadioButton();
            this.radioBoth = new System.Windows.Forms.RadioButton();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.btnAddCircle = new System.Windows.Forms.Button();
            this.btnCircleSizeIncrease = new System.Windows.Forms.Button();
            this.btnCircleSizeDecrease = new System.Windows.Forms.Button();
            this.lblCircleSize = new System.Windows.Forms.Label();
            this.telescopeControlPanel = new System.Windows.Forms.Panel();
            this.trackBarStepsPerSecond = new System.Windows.Forms.TrackBar();
            this.lblStepsPerSecond = new System.Windows.Forms.Label();
            this.lblStepsPerSecondValue = new System.Windows.Forms.Label();
            this.trackBarFocusSpeed = new System.Windows.Forms.TrackBar();
            this.lblFocusSpeed = new System.Windows.Forms.Label();
            this.lblFocusSpeedValue = new System.Windows.Forms.Label();
            this.btnSaveFrame = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.videoPanel.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.telescopeControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraSettingsToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // cameraSettingsToolStripMenuItem
            // 
            this.cameraSettingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainCameraToolStripMenuItem,
            this.secondaryCameraToolStripMenuItem,
            this.calibrationToolStripMenuItem});
            this.cameraSettingsToolStripMenuItem.Name = "cameraSettingsToolStripMenuItem";
            this.cameraSettingsToolStripMenuItem.Size = new System.Drawing.Size(105, 20);
            this.cameraSettingsToolStripMenuItem.Text = "Camera Settings";
            // 
            // mainCameraToolStripMenuItem
            // 
            this.mainCameraToolStripMenuItem.Name = "mainCameraToolStripMenuItem";
            this.mainCameraToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.mainCameraToolStripMenuItem.Text = "Main Camera";
            this.mainCameraToolStripMenuItem.Click += new System.EventHandler(this.BtnMainCameraControl_Click);
            // 
            // secondaryCameraToolStripMenuItem
            // 
            this.secondaryCameraToolStripMenuItem.Name = "secondaryCameraToolStripMenuItem";
            this.secondaryCameraToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.secondaryCameraToolStripMenuItem.Text = "Secondary Camera";
            this.secondaryCameraToolStripMenuItem.Click += new System.EventHandler(this.BtnSecondaryCameraControl_Click);
            // 
            // calibrationToolStripMenuItem
            // 
            this.calibrationToolStripMenuItem.Name = "calibrationToolStripMenuItem";
            this.calibrationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.calibrationToolStripMenuItem.Text = "Calibration";
            this.calibrationToolStripMenuItem.Click += new System.EventHandler(this.BtnCalibration_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plateSolverToolStripMenuItem,
            this.starFollowerToolStripMenuItem,
            this.starFollower2ToolStripMenuItem,
            this.starFollower3ToolStripMenuItem,
            this.siderealTrackerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // plateSolverToolStripMenuItem
            // 
            this.plateSolverToolStripMenuItem.Name = "plateSolverToolStripMenuItem";
            this.plateSolverToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.plateSolverToolStripMenuItem.Text = "Plate Solver";
            this.plateSolverToolStripMenuItem.Click += new System.EventHandler(this.PlateSolverToolStripMenuItem_Click);
            // 
            // starFollowerToolStripMenuItem
            // 
            this.starFollowerToolStripMenuItem.Name = "starFollowerToolStripMenuItem";
            this.starFollowerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.starFollowerToolStripMenuItem.Text = "Star Follower";
            this.starFollowerToolStripMenuItem.Click += new System.EventHandler(this.StarFollowerToolStripMenuItem_Click);
            // 
            // starFollower2ToolStripMenuItem
            // 
            this.starFollower2ToolStripMenuItem.Name = "starFollower2ToolStripMenuItem";
            this.starFollower2ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.starFollower2ToolStripMenuItem.Text = "Star Follower 2";
            this.starFollower2ToolStripMenuItem.Click += new System.EventHandler(this.StarFollower2ToolStripMenuItem_Click);
            // 
            // starFollower3ToolStripMenuItem
            // 
            this.starFollower3ToolStripMenuItem.Name = "starFollower3ToolStripMenuItem";
            this.starFollower3ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.starFollower3ToolStripMenuItem.Text = "Star Follower 3";
            this.starFollower3ToolStripMenuItem.Click += new System.EventHandler(this.StarFollower3ToolStripMenuItem_Click);
            // 
            // siderealTrackerToolStripMenuItem
            // 
            this.siderealTrackerToolStripMenuItem.Name = "siderealTrackerToolStripMenuItem";
            this.siderealTrackerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.siderealTrackerToolStripMenuItem.Text = "Sidereal Tracker";
            this.siderealTrackerToolStripMenuItem.Click += new System.EventHandler(this.SiderealTrackerToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
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
            this.videoPanel.Controls.Add(this.pictureBox1);
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
            // chkMainFlipH
            // 
            this.chkMainFlipH.AutoSize = true;
            this.chkMainFlipH.Checked = true;
            this.chkMainFlipH.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMainFlipH.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkMainFlipH.ForeColor = System.Drawing.Color.White;
            this.chkMainFlipH.Location = new System.Drawing.Point(380, 8);
            this.chkMainFlipH.Name = "chkMainFlipH";
            this.chkMainFlipH.Text = "Main H";
            this.chkMainFlipH.UseVisualStyleBackColor = true;
            this.chkMainFlipH.CheckedChanged += new System.EventHandler(this.ChkMainFlipH_CheckedChanged);
            // 
            // chkMainFlipV
            // 
            this.chkMainFlipV.AutoSize = true;
            this.chkMainFlipV.Checked = true;
            this.chkMainFlipV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMainFlipV.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkMainFlipV.ForeColor = System.Drawing.Color.White;
            this.chkMainFlipV.Location = new System.Drawing.Point(440, 8);
            this.chkMainFlipV.Name = "chkMainFlipV";
            this.chkMainFlipV.Text = "Main V";
            this.chkMainFlipV.UseVisualStyleBackColor = true;
            this.chkMainFlipV.CheckedChanged += new System.EventHandler(this.ChkMainFlipV_CheckedChanged);
            // 
            // chkSecFlipH
            // 
            this.chkSecFlipH.AutoSize = true;
            this.chkSecFlipH.Checked = true;
            this.chkSecFlipH.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSecFlipH.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkSecFlipH.ForeColor = System.Drawing.Color.White;
            this.chkSecFlipH.Location = new System.Drawing.Point(500, 8);
            this.chkSecFlipH.Name = "chkSecFlipH";
            this.chkSecFlipH.Text = "Sec H";
            this.chkSecFlipH.UseVisualStyleBackColor = true;
            this.chkSecFlipH.CheckedChanged += new System.EventHandler(this.ChkSecFlipH_CheckedChanged);
            // 
            // chkSecFlipV
            // 
            this.chkSecFlipV.AutoSize = true;
            this.chkSecFlipV.Checked = true;
            this.chkSecFlipV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSecFlipV.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkSecFlipV.ForeColor = System.Drawing.Color.White;
            this.chkSecFlipV.Location = new System.Drawing.Point(560, 8);
            this.chkSecFlipV.Name = "chkSecFlipV";
            this.chkSecFlipV.Text = "Sec V";
            this.chkSecFlipV.UseVisualStyleBackColor = true;
            this.chkSecFlipV.CheckedChanged += new System.EventHandler(this.ChkSecFlipV_CheckedChanged);
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
            this.controlPanel.Controls.Add(this.chkMainFlipH);
            this.controlPanel.Controls.Add(this.chkMainFlipV);
            this.controlPanel.Controls.Add(this.chkSecFlipH);
            this.controlPanel.Controls.Add(this.chkSecFlipV);
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
            // telescopeControlPanel
            // 
            this.telescopeControlPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.telescopeControlPanel.Controls.Add(this.lblStepsPerSecond);
            this.telescopeControlPanel.Controls.Add(this.trackBarStepsPerSecond);
            this.telescopeControlPanel.Controls.Add(this.lblStepsPerSecondValue);
            this.telescopeControlPanel.Controls.Add(this.lblFocusSpeed);
            this.telescopeControlPanel.Controls.Add(this.trackBarFocusSpeed);
            this.telescopeControlPanel.Controls.Add(this.lblFocusSpeedValue);
            this.telescopeControlPanel.Controls.Add(this.btnSaveFrame);
            this.telescopeControlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.telescopeControlPanel.Height = 80;
            this.telescopeControlPanel.Name = "telescopeControlPanel";
            this.telescopeControlPanel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.telescopeControlPanel.Resize += new System.EventHandler((s, e) => { PositionFocusControls(); PositionSaveFrameButton(); });
            // 
            // trackBarStepsPerSecond
            // 
            this.trackBarStepsPerSecond.LargeChange = 1;
            this.trackBarStepsPerSecond.Location = new System.Drawing.Point(10, 30);
            this.trackBarStepsPerSecond.Maximum = 5;
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
            // btnSaveFrame
            // 
            this.btnSaveFrame.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnSaveFrame.AutoSize = true;
            this.btnSaveFrame.BackColor = System.Drawing.Color.DimGray;
            this.btnSaveFrame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveFrame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveFrame.ForeColor = System.Drawing.Color.White;
            this.btnSaveFrame.Location = new System.Drawing.Point(500, 35); // Initial pos, will be repositioned
            this.btnSaveFrame.Name = "btnSaveFrame";
            this.btnSaveFrame.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnSaveFrame.Text = "Save Frame";
            this.btnSaveFrame.UseVisualStyleBackColor = false;
            this.btnSaveFrame.Click += new System.EventHandler(this.BtnSaveFrame_Click);
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
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.videoPanel.ResumeLayout(false);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.telescopeControlPanel.ResumeLayout(false);
            this.telescopeControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarStepsPerSecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFocusSpeed)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
            this.BackColor = System.Drawing.Color.Black;
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFrameInfo1;
        private System.Windows.Forms.Label lblFrameInfo2;
        private System.Windows.Forms.CheckBox chkMainFlipH;
        private System.Windows.Forms.CheckBox chkMainFlipV;
        private System.Windows.Forms.CheckBox chkSecFlipH;
        private System.Windows.Forms.CheckBox chkSecFlipV;
        private System.Windows.Forms.RadioButton radioMainOnly;
        private System.Windows.Forms.RadioButton radioSecondaryOnly;
        private System.Windows.Forms.RadioButton radioBoth;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Button btnAddCircle;
        private System.Windows.Forms.Button btnCircleSizeIncrease;
        private System.Windows.Forms.Button btnCircleSizeDecrease;
        private System.Windows.Forms.Label lblCircleSize;
        // private System.Windows.Forms.Button btnMainCameraControl; // Removed
        // private System.Windows.Forms.Button btnSecondaryCameraControl; // Removed
        private System.Windows.Forms.Panel telescopeControlPanel;
        private System.Windows.Forms.TrackBar trackBarStepsPerSecond;
        private System.Windows.Forms.Label lblStepsPerSecond;
        private System.Windows.Forms.Label lblStepsPerSecondValue;
        private System.Windows.Forms.TrackBar trackBarFocusSpeed;
        private System.Windows.Forms.Label lblFocusSpeed;
        private System.Windows.Forms.Label lblFocusSpeedValue;
        private System.Windows.Forms.Button btnSaveFrame;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cameraSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mainCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem secondaryCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem plateSolverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starFollowerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starFollower2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starFollower3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem siderealTrackerToolStripMenuItem;
    }
}