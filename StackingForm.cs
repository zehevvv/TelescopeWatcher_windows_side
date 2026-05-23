using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    public partial class StackingForm : Form
    {
        private readonly VideoPlayerForm _videoPlayerForm;
        private volatile bool _isStacking = false;
        private int _capturedFrames = 0;
        private string? _saveFolder = null;
        private DateTime _stackingStartTime;
        private System.Windows.Forms.Timer _uiTimer;

        // Cached at start so background thread can read safely
        private bool _modeIsTimeBased;
        private double _targetSeconds;
        private int _targetFrames;

        // Producer/consumer queue: background thread saves without touching UI
        private BlockingCollection<(Image image, string path)>? _saveQueue;
        private Task? _saveWorker;
        private CancellationTokenSource? _saveCts;

        public StackingForm(VideoPlayerForm videoPlayerForm)
        {
            _videoPlayerForm = videoPlayerForm;
            InitializeComponent();

            UpdateFpsLabel();

            radioTimeBased.CheckedChanged += RadioMode_CheckedChanged;
            radioFrameBased.CheckedChanged += RadioMode_CheckedChanged;
            numSeconds.ValueChanged += NumSeconds_ValueChanged;
            numFrames.ValueChanged += NumFrames_ValueChanged;

            _uiTimer = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 500;
            _uiTimer.Tick += UiTimer_Tick;

            UpdateModeUI();
        }

        private void UpdateFpsLabel()
        {
            double fps = _videoPlayerForm.CurrentFps1;
            lblFps.Text = fps > 0 ? $"Main camera FPS: {fps:F1}" : "Main camera FPS: (measuring...)";
            UpdateEstimateLabels();
        }

        private void RadioMode_CheckedChanged(object? sender, EventArgs e) => UpdateModeUI();

        private void UpdateModeUI()
        {
            bool timeBased = radioTimeBased.Checked;
            numSeconds.Enabled = timeBased;
            lblFramesEstimate.Visible = timeBased;
            numFrames.Enabled = !timeBased;
            lblTimeEstimate.Visible = !timeBased;
            UpdateEstimateLabels();
        }

        private void NumSeconds_ValueChanged(object? sender, EventArgs e) => UpdateEstimateLabels();
        private void NumFrames_ValueChanged(object? sender, EventArgs e) => UpdateEstimateLabels();

        private void UpdateEstimateLabels()
        {
            double fps = _videoPlayerForm.CurrentFps1;
            if (fps <= 0) fps = 1;

            if (radioTimeBased.Checked)
            {
                int frames = (int)((double)numSeconds.Value * fps);
                lblFramesEstimate.Text = $"≈ {frames} frames will be saved";
            }
            else
            {
                double secs = (int)numFrames.Value / fps;
                lblTimeEstimate.Text = $"≈ {secs:F1} seconds";
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = "Select folder to save stacked frames";
            if (fbd.ShowDialog() != DialogResult.OK) return;

            int targetFrames = GetTargetFrameCount();
            string subFolderName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{targetFrames}f";
            _saveFolder = Path.Combine(fbd.SelectedPath, subFolderName);
            Directory.CreateDirectory(_saveFolder);

            _modeIsTimeBased = radioTimeBased.Checked;
            _targetSeconds = (double)numSeconds.Value;
            _targetFrames = (int)numFrames.Value;

            _capturedFrames = 0;
            _stackingStartTime = DateTime.Now;

            // Start save worker before setting flag
            _saveCts = new CancellationTokenSource();
            _saveQueue = new BlockingCollection<(Image, string)>(boundedCapacity: 500);
            _saveWorker = Task.Run(SaveWorkerLoop);

            _isStacking = true;

            // Disable only stacking form controls (NOT VideoPlayerForm — disabling it
            // stops its message pump and prevents BeginInvoke from processing)
            radioTimeBased.Enabled = false;
            radioFrameBased.Enabled = false;
            numSeconds.Enabled = false;
            numFrames.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            lblProgress.Text = "Stacking: 0 frames | 0s";

            // Subscribe directly to the MJPEG client — fires on the stream background thread,
            // completely independent of the UI message pump
            _videoPlayerForm.MainMjpegClient.FrameReceived += OnMjpegFrameReceived;
            _uiTimer.Start();
        }

        private int GetTargetFrameCount()
        {
            double fps = _videoPlayerForm.CurrentFps1;
            if (fps <= 0) fps = 1;
            return _modeIsTimeBased ? (int)(_targetSeconds * fps) : (int)numFrames.Value;
        }

        private void OnMjpegFrameReceived(object? sender, Image image)
        {
            if (!_isStacking) return;

            int frameIndex = Interlocked.Increment(ref _capturedFrames);

            // Clone on the calling thread (stream background thread) — safe since
            // VideoPlayerForm also clones before storing in _pendingFrame1
            Image clone;
            try { clone = (Image)image.Clone(); }
            catch { return; }

            string savePath = Path.Combine(_saveFolder!, $"{frameIndex}.png");
            _saveQueue?.TryAdd((clone, savePath));

            // Check stop condition
            bool shouldStop = _modeIsTimeBased
                ? (DateTime.Now - _stackingStartTime).TotalSeconds >= _targetSeconds
                : frameIndex >= _targetFrames;

            if (shouldStop)
                this.BeginInvoke(new Action(StopStacking));
        }

        private void SaveWorkerLoop()
        {
            try
            {
                foreach (var (image, path) in _saveQueue!.GetConsumingEnumerable())
                {
                    try { image.Save(path, ImageFormat.Png); }
                    catch { }
                    finally { image.Dispose(); }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void BtnStop_Click(object? sender, EventArgs e) => StopStacking();

        private void StopStacking()
        {
            if (!_isStacking) return;
            _isStacking = false;

            _videoPlayerForm.MainMjpegClient.FrameReceived -= OnMjpegFrameReceived;
            _uiTimer.Stop();

            // Signal no more items; worker will drain remaining frames before exiting
            _saveQueue?.CompleteAdding();

            btnStop.Enabled = false;
            btnStart.Enabled = true;
            radioTimeBased.Enabled = true;
            radioFrameBased.Enabled = true;
            UpdateModeUI();

            int saved = _capturedFrames;
            double elapsed = (DateTime.Now - _stackingStartTime).TotalSeconds;

            // Wait for all queued frames to be written, then notify
            Task.Run(async () =>
            {
                if (_saveWorker != null)
                    await _saveWorker.ConfigureAwait(false);

                this.BeginInvoke(new Action(() =>
                {
                    lblProgress.Text = $"Done: {saved} frames in {elapsed:F1}s → {_saveFolder}";
                    MessageBox.Show($"Stacking complete!\n{saved} frames saved to:\n{_saveFolder}",
                        "Stacking Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            });
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isStacking) return;
            double elapsed = (DateTime.Now - _stackingStartTime).TotalSeconds;
            int frames = _capturedFrames;
            lblProgress.Text = _modeIsTimeBased
                ? $"Stacking: {frames} frames | {elapsed:F1}s / {_targetSeconds:F0}s"
                : $"Stacking: {frames} / {_targetFrames} frames | {elapsed:F1}s";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isStacking) StopStacking();
            _uiTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
