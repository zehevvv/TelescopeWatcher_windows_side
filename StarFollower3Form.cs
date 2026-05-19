using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    /// <summary>
    /// Star Follower 3 - drift tracking via phase correlation.
    ///
    /// Workflow:
    ///   1. Press "Capture Ref": takes a snapshot, finds up to 5 brightest stars
    ///      (brightness > 180), blacks out all other pixels and stores the result
    ///      as the reference mask.
    ///   2. Press "Start": on every tick the same masking is applied to a fresh
    ///      frame; PhaseCorrelation.EstimateOffset() returns (dx, dy) in pixels.
    ///   3. The offset is converted to a percentage and motor correction commands
    ///      are issued using the same ScaleCommands / SendMove logic as
    ///      StarFollower2Form.
    ///
    /// Motor direction mapping (same as StarFollower2Form):
    ///   UP    -> v=1, d=1
    ///   DOWN  -> v=1, d=0
    ///   LEFT  -> v=0, d=0
    ///   RIGHT -> v=0, d=1
    /// </summary>
    public partial class StarFollower3Form : Form
    {
        private readonly TelescopeController _controller;
        private readonly string _primaryStreamUrl;
        private readonly string _secondaryStreamUrl;

        private CancellationTokenSource? _cts;
        private Bitmap? _referenceMask;   // star-only masked reference frame

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        // ------------------------------------------------------------------
        // Star detection constants
        // ------------------------------------------------------------------
        private const int    TopStarCount  = 5;   // maximum stars to keep
        private const int    MinBrightness = 180;  // minimum brightness to qualify
        private const int    NmsHalf       = 5;    // non-maximum suppression window
        private const int    StarBlobRadius = 5;   // pixel radius drawn around each star in the mask

        public StarFollower3Form(TelescopeController controller,
                                  string primaryStreamUrl,
                                  string secondaryStreamUrl)
        {
            _controller        = controller;
            _primaryStreamUrl  = primaryStreamUrl;
            _secondaryStreamUrl = secondaryStreamUrl;

            InitializeComponent();

            cbCamera.Items.Clear();
            cbCamera.Items.Add("Primary");
            cbCamera.Items.Add("Secondary");
            cbCamera.SelectedIndex = 0;

            this.FormClosing += StarFollower3Form_FormClosing;
        }

        private void StarFollower3Form_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopTracking();
            _referenceMask?.Dispose();
            _referenceMask = null;
        }

        // ------------------------------------------------------------------
        // UI events
        // ------------------------------------------------------------------

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (_referenceMask == null)
            {
                MessageBox.Show("Please capture a reference frame first.",
                    "No Reference", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StopTracking();

            double duration  = (double)numDuration.Value;
            double threshold = (double)numThreshold.Value;
            int    baseSteps = (int)numStepsCmd.Value;
            int    baseSpeed = (int)numSpeedCmd.Value;
            string streamUrl = cbCamera.SelectedIndex == 0 ? _primaryStreamUrl : _secondaryStreamUrl;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Run(() => TrackingLoop(duration, threshold, baseSteps, baseSpeed, streamUrl, token), token);

            UpdateStatus(true);
            AppendOutput("Tracking started.");
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopTracking();
            AppendOutput("Tracking stopped.");
        }

        private async void BtnCaptureRef_Click(object sender, EventArgs e)
        {
            string streamUrl = cbCamera.SelectedIndex == 0 ? _primaryStreamUrl : _secondaryStreamUrl;
            AppendOutput("Capturing reference frame...");
            btnCaptureRef.Enabled = false;
            try
            {
                using var raw = await FetchFrameAsync(streamUrl);
                if (raw == null)
                {
                    AppendOutput("Reference capture: failed to fetch frame.");
                    return;
                }

                var stars = FindTopStars(raw);
                if (stars.Count == 0)
                {
                    AppendOutput($"Reference capture: no stars with brightness >{MinBrightness} found.");
                    return;
                }

                _referenceMask?.Dispose();
                _referenceMask = BuildStarMask(raw, stars);

                UpdateRefStatus(true);
                AppendOutput($"Reference captured: {stars.Count} star(s) " +
                             $"at {string.Join(", ", stars.Select(s => $"({s.X},{s.Y}) b={s.Brightness}"))}");
            }
            catch (Exception ex)
            {
                AppendOutput($"Reference capture error: {ex.Message}");
            }
            finally
            {
                btnCaptureRef.Enabled = true;
            }
        }

        private async void BtnDebug_Click(object sender, EventArgs e)
        {
            string streamUrl = cbCamera.SelectedIndex == 0 ? _primaryStreamUrl : _secondaryStreamUrl;
            AppendOutput("Capturing debug frame...");
            try
            {
                using var raw = await FetchFrameAsync(streamUrl);
                if (raw == null)
                {
                    AppendOutput("Debug: failed to capture frame.");
                    return;
                }

                var stars = FindTopStars(raw);
                if (stars.Count == 0)
                {
                    AppendOutput($"Debug: no stars with brightness >{MinBrightness} found in {raw.Width}x{raw.Height} frame.");
                    return;
                }

                AppendOutput($"Debug: {stars.Count} star(s) in {raw.Width}x{raw.Height} frame:");
                foreach (var s in stars)
                    AppendOutput($"  ({s.X},{s.Y})  brightness={s.Brightness}");

                if (_referenceMask != null)
                {
                    using var mask = BuildStarMask(raw, stars);
                    var offset = PhaseCorrelation.EstimateOffset(_referenceMask, mask, scale: 0.25f);
                    if (offset.HasValue)
                        AppendOutput($"Debug offset vs reference: dx={offset.Value.dx}px  dy={offset.Value.dy}px");
                    else
                        AppendOutput("Debug: phase correlation returned null (size mismatch?).");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Debug error: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------
        // Tracking loop (background thread)
        // ------------------------------------------------------------------

        private async Task TrackingLoop(double durationSec, double thresholdPct,
                                         int baseSteps, int baseSpeed,
                                         string streamUrl, CancellationToken ct)
        {
            AppendOutput("Tracking loop started.");
            var _loopSw = new System.Diagnostics.Stopwatch();
            while (!ct.IsCancellationRequested)
            {
                _loopSw.Restart();
                try
                {
                    var fetchSw = System.Diagnostics.Stopwatch.StartNew();
                    using var raw = await FetchFrameAsync(streamUrl);
                    fetchSw.Stop();
                    //AppendOutput($"[TIMING] FetchFrame={fetchSw.ElapsedMilliseconds}ms");
                    if (raw == null)
                    {
                        AppendOutput("Warning: could not capture frame, retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                        continue;
                    }

                    int w = raw.Width, h = raw.Height;
                    var procSw = System.Diagnostics.Stopwatch.StartNew();
                    var stars = FindTopStars(raw);

                    if (stars.Count == 0)
                    {
                        AppendOutput("No qualifying stars detected, skipping correction.");
                        await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                        continue;
                    }

                    (int dx, int dy, float mean)? offset;
                    using (var mask = BuildStarMask(raw, stars))
                    {
                        // _referenceMask is read-only after capture so no lock needed
                        offset = PhaseCorrelation.EstimateOffset(_referenceMask!, mask, scale: 0.25f);
                    }
                    procSw.Stop();
                    //AppendOutput($"[TIMING] Processing={procSw.ElapsedMilliseconds}ms");                    

                    if (offset == null)
                    {
                        AppendOutput("Phase correlation failed (frame size changed?), skipping.");
                        await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                        continue;
                    }

                    AppendOutput($"mean {offset.Value.mean}");

                    double offsetXPct = offset.Value.dx / (double)w * 100.0;
                    double offsetYPct = offset.Value.dy / (double)h * 100.0;
                    double absX       = Math.Abs(offsetXPct);
                    double absY       = Math.Abs(offsetYPct);

                    if (absX <= thresholdPct && absY <= thresholdPct)
                    {
                        AppendOutput($"Centred (X={offsetXPct:F1}%, Y={offsetYPct:F1}%), no correction.");
                    }
                    else
                    {
                        double maxOffset = Math.Max(absX, absY);
                        (string stepsCmd, string speedCmd) = ScaleCommands(baseSteps, baseSpeed, maxOffset);

                        if (absX >= absY)
                        {
                            // Stars drifted RIGHT (dx>0) → image is flipped, so correct LEFT
                            bool goRight  = offset.Value.dx > 0;
                            string dirCmd = goRight ? "v=0\nd=0" : "v=0\nd=1";
                            string name   = goRight ? "LEFT"     : "RIGHT";
                            AppendOutput($"Correcting {name} (X={offsetXPct:F1}%, Y={offsetYPct:F1}%)");
                            SendMove(speedCmd, stepsCmd, dirCmd);
                        }
                        else
                        {
                            bool goDown   = offset.Value.dy > 0;
                            string dirCmd = goDown ? "v=1\nd=1" : "v=1\nd=0";
                            string name   = goDown ? "UP"       : "DOWN";
                            AppendOutput($"Correcting {name} (X={offsetXPct:F1}%, Y={offsetYPct:F1}%)");
                            SendMove(speedCmd, stepsCmd, dirCmd);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AppendOutput($"Loop error: {ex.Message}");
                }

                _loopSw.Stop();
                //AppendOutput($"[TIMING] CycleTotal={_loopSw.ElapsedMilliseconds}ms  (excl. delay)");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            AppendOutput("Tracking loop ended.");
            UpdateStatus(false);
        }

        // ------------------------------------------------------------------
        // Motor command helpers  (identical to StarFollower2Form)
        // ------------------------------------------------------------------

        private static (string stepsCmd, string speedCmd) ScaleCommands(int baseSteps, int baseSpeed, double maxOffsetPct)
        {
            int steps; int speed;
            if      (maxOffsetPct > 50) { steps = baseSteps * 10; speed = Math.Max(1, baseSpeed / 10); }
            else if (maxOffsetPct > 25) { steps = baseSteps * 5;  speed = Math.Max(1, baseSpeed / 5);  }
            else if (maxOffsetPct > 5)  { steps = baseSteps * 2;  speed = Math.Max(1, baseSpeed / 2);  }
            else                        { steps = baseSteps;       speed = baseSpeed;                   }
            return ($"s={steps}", $"t={speed}");
        }

        private void SendMove(string speedCmd, string stepsCmd, string directionCmd)
        {
            _controller.SendRawCommand(speedCmd);
            _controller.SendRawCommand(stepsCmd);
            foreach (var line in directionCmd.Split('\n'))
            {
                string cmd = line.Trim();
                if (!string.IsNullOrEmpty(cmd))
                    _controller.SendRawCommand(cmd);
            }
        }

        // ------------------------------------------------------------------
        // Star detection
        // ------------------------------------------------------------------

        private struct StarResult
        {
            public int X, Y, Brightness;
        }

        /// <summary>
        /// Finds up to <see cref="TopStarCount"/> local-maximum pixels whose
        /// brightness exceeds <see cref="MinBrightness"/>, returned brightest-first.
        /// </summary>
        private static List<StarResult> FindTopStars(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var rect   = new Rectangle(0, 0, w, h);
            var data   = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            int bytes  = Math.Abs(stride) * h;
            var buf    = new byte[bytes];
            Marshal.Copy(data.Scan0, buf, 0, bytes);
            bmp.UnlockBits(data);

            byte[] gray = new byte[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    gray[y * w + x] = buf[y * stride + x * 4 + 2]; // R == lum

            var candidates = new List<StarResult>();

            for (int y = NmsHalf; y < h - NmsHalf; y++)
            {
                for (int x = NmsHalf; x < w - NmsHalf; x++)
                {
                    byte val = gray[y * w + x];
                    if (val <= MinBrightness) continue;

                    bool isMax = true;
                    for (int ky = -NmsHalf; ky <= NmsHalf && isMax; ky++)
                        for (int kx = -NmsHalf; kx <= NmsHalf && isMax; kx++)
                        {
                            if (ky == 0 && kx == 0) continue;
                            if (gray[(y + ky) * w + (x + kx)] >= val) isMax = false;
                        }

                    if (isMax)
                        candidates.Add(new StarResult { X = x, Y = y, Brightness = val });
                }
            }

            // Return the brightest TopStarCount stars
            return candidates
                .OrderByDescending(s => s.Brightness)
                .Take(TopStarCount)
                .ToList();
        }

        /// <summary>
        /// Creates a black bitmap the same size as <paramref name="src"/> and
        /// draws a bright blob at each star position so that phase correlation
        /// has a clear signal to lock onto.
        /// </summary>
        private static Bitmap BuildStarMask(Bitmap src, List<StarResult> stars)
        {
            int w = src.Width, h = src.Height;
            var mask = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, w, h);

            var dstData = mask.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int stride  = dstData.Stride;
            var buf     = new byte[Math.Abs(stride) * h];

            foreach (var star in stars)
            {
                int br = star.Brightness;
                for (int ky = -StarBlobRadius; ky <= StarBlobRadius; ky++)
                {
                    int py = star.Y + ky;
                    if (py < 0 || py >= h) continue;
                    for (int kx = -StarBlobRadius; kx <= StarBlobRadius; kx++)
                    {
                        if (kx * kx + ky * ky > StarBlobRadius * StarBlobRadius) continue;
                        int px = star.X + kx;
                        if (px < 0 || px >= w) continue;
                        int idx = py * stride + px * 4;
                        buf[idx]     = (byte)br; // B
                        buf[idx + 1] = (byte)br; // G
                        buf[idx + 2] = (byte)br; // R
                        buf[idx + 3] = 255;       // A
                    }
                }
            }

            Marshal.Copy(buf, 0, dstData.Scan0, buf.Length);
            mask.UnlockBits(dstData);
            return mask;
        }

        // ------------------------------------------------------------------
        // Frame capture  (identical to StarFollower2Form)
        // ------------------------------------------------------------------

        private static async Task<Bitmap?> FetchFrameAsync(string streamUrl)
        {
            string snapshotUrl = streamUrl.Replace("action=stream", "action=snapshot");
            if (snapshotUrl == streamUrl)
            {
                try
                {
                    var uri = new Uri(streamUrl);
                    snapshotUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/?action=snapshot";
                }
                catch { /* keep original */ }
            }

            try
            {
                var bytes = await _http.GetByteArrayAsync(snapshotUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                var bmp = new Bitmap(ms);
                return ToGrayscale(bmp);
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap ToGrayscale(Bitmap src)
        {
            int w = src.Width, h = src.Height;
            var gray = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, w, h);

            var srcData = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride  = srcData.Stride;
            int bytes   = Math.Abs(stride) * h;
            var buf     = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buf, 0, bytes);
            src.UnlockBits(srcData);

            for (int i = 0; i < bytes; i += 4)
            {
                byte lum   = (byte)(0.2126 * buf[i + 2] + 0.7152 * buf[i + 1] + 0.0722 * buf[i]);
                buf[i]     = lum;
                buf[i + 1] = lum;
                buf[i + 2] = lum;
                buf[i + 3] = 255;
            }

            var dstData = gray.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buf, 0, dstData.Scan0, bytes);
            gray.UnlockBits(dstData);

            src.Dispose();
            return gray;
        }

        // ------------------------------------------------------------------
        // UI thread helpers
        // ------------------------------------------------------------------

        private void StopTracking()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            UpdateStatus(false);
        }

        private void UpdateStatus(bool active)
        {
            if (lblActiveStatus.InvokeRequired)
            {
                lblActiveStatus.Invoke(new Action(() => UpdateStatus(active)));
                return;
            }
            lblActiveStatus.Text      = active ? "Status: Active"  : "Status: Stopped";
            lblActiveStatus.ForeColor = active ? Color.DarkGreen   : Color.DarkRed;
        }

        private void UpdateRefStatus(bool captured)
        {
            if (lblRefStatus.InvokeRequired)
            {
                lblRefStatus.Invoke(new Action(() => UpdateRefStatus(captured)));
                return;
            }
            lblRefStatus.Text      = captured ? "Ref: Captured" : "Ref: Not set";
            lblRefStatus.ForeColor = captured ? Color.DarkGreen  : Color.DarkRed;
        }

        private void AppendOutput(string text)
        {
            if (txtOutput.InvokeRequired)
            {
                txtOutput.Invoke(new Action(() => AppendOutput(text)));
                return;
            }
            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}\r\n");
        }
    }
}
