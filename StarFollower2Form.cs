using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelescopeWatcher
{
    /// <summary>
    /// Star Follower 2 - autonomous local star tracking without a server.
    /// Captures frames from the MJPEG snapshot endpoint, detects the brightest
    /// star and issues motor correction commands via TelescopeController.
    ///
    /// Motor direction mapping (mirrors StarFollower.py):
    ///   UP    -> v=1, d=1
    ///   DOWN  -> v=1, d=0
    ///   LEFT  -> v=0, d=0
    ///   RIGHT -> v=0, d=1
    /// </summary>
    public partial class StarFollower2Form : Form
    {
        private readonly TelescopeController _controller;
        private readonly string _primaryStreamUrl;
        private readonly string _secondaryStreamUrl;

        private CancellationTokenSource? _cts;

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        public StarFollower2Form(TelescopeController controller,
                                  string primaryStreamUrl,
                                  string secondaryStreamUrl)
        {
            _controller = controller;
            _primaryStreamUrl = primaryStreamUrl;
            _secondaryStreamUrl = secondaryStreamUrl;

            InitializeComponent();

            cbCamera.Items.Clear();
            cbCamera.Items.Add("Primary");
            cbCamera.Items.Add("Secondary");
            cbCamera.SelectedIndex = 0;

            this.FormClosing += StarFollower2Form_FormClosing;
        }

        private void StarFollower2Form_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopTracking();
        }

        // ------------------------------------------------------------------
        // UI events
        // ------------------------------------------------------------------

        private void BtnStart_Click(object sender, EventArgs e)
        {
            StopTracking();

            double duration  = (double)numDuration.Value;
            double threshold = (double)numThreshold.Value;
            string stepsCmd  = $"s={(int)numStepsCmd.Value}";
            string speedCmd  = $"t={(int)numSpeedCmd.Value}";
            bool usePrimary  = cbCamera.SelectedIndex == 0;
            string streamUrl = usePrimary ? _primaryStreamUrl : _secondaryStreamUrl;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Run(() => TrackingLoop(duration, threshold, stepsCmd, speedCmd, streamUrl, token), token);

            UpdateStatus(true);
            AppendOutput("Tracking started.");
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopTracking();
            AppendOutput("Tracking stopped.");
        }

        private async void BtnDebug_Click(object sender, EventArgs e)
        {
            bool usePrimary  = cbCamera.SelectedIndex == 0;
            string streamUrl = usePrimary ? _primaryStreamUrl : _secondaryStreamUrl;

            AppendOutput("Capturing debug frame...");
            try
            {
                using var bmp = await FetchFrameAsync(streamUrl);
                if (bmp == null)
                {
                    AppendOutput("Debug: failed to capture frame.");
                    return;
                }

                int w = bmp.Width, h = bmp.Height;
                var star = FindBrightestStar(bmp);
                if (star == null)
                {
                    AppendOutput($"Debug: no star found in {w}x{h} frame.");
                    return;
                }

                double offX = (star.Value.X - w / 2.0) / w * 100.0;
                double offY = (star.Value.Y - h / 2.0) / h * 100.0;
                AppendOutput($"Debug: star at ({star.Value.X},{star.Value.Y})  brightness={star.Value.Brightness}  " +
                             $"offset X={offX:F1}%  Y={offY:F1}%  frame {w}x{h}");
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
                                         string stepsCmd, string speedCmd,
                                         string streamUrl, CancellationToken ct)
        {
            AppendOutput("Tracking loop started.");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var bmp = await FetchFrameAsync(streamUrl);
                    if (bmp == null)
                    {
                        AppendOutput("Warning: could not capture frame, retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                        continue;
                    }

                    int w = bmp.Width, h = bmp.Height;
                    var star = FindBrightestStar(bmp);

                    if (star == null)
                    {
                        AppendOutput("No star detected, skipping correction.");
                        await Task.Delay(TimeSpan.FromSeconds(durationSec), ct);
                        continue;
                    }

                    double dx         = star.Value.X - w / 2.0;
                    double dy         = star.Value.Y - h / 2.0;
                    double offsetXPct = dx / w * 100.0;
                    double offsetYPct = dy / h * 100.0;
                    double absX       = Math.Abs(offsetXPct);
                    double absY       = Math.Abs(offsetYPct);

                    if (absX <= thresholdPct && absY <= thresholdPct)
                    {
                        AppendOutput($"Star centred (X={offsetXPct:F1}%, Y={offsetYPct:F1}%), no correction.");
                    }
                    else if (absX >= absY)
                    {
                        bool goRight  = dx > 0;
                        string dirCmd = goRight ? "v=0\nd=0" : "v=0\nd=1";
                        string name   = goRight ? "RIGHT"    : "LEFT";
                        AppendOutput($"Correcting {name} (X={offsetXPct:F1}%)");
                        SendMove(speedCmd, stepsCmd, dirCmd);
                    }
                    else
                    {
                        bool goDown   = dy > 0;
                        string dirCmd = goDown ? "v=1\nd=1" : "v=1\nd=0";
                        string name   = goDown ? "DOWN"     : "UP";
                        AppendOutput($"Correcting {name} (Y={offsetYPct:F1}%)");
                        SendMove(speedCmd, stepsCmd, dirCmd);
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
        // Motor command helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Sends speed -> steps -> direction command sequence.
        /// The direction string may contain multiple newline-separated commands (e.g. "v=1\nd=1").
        /// </summary>
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
        // Frame capture
        // ------------------------------------------------------------------

        private static async Task<Bitmap?> FetchFrameAsync(string streamUrl)
        {
            // Derive snapshot URL from the MJPEG stream URL.
            // Typical pattern: http://host:port/?action=stream -> action=snapshot
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
        // Star detection - brightest local maximum (port of StarFinder.py NMS)
        // ------------------------------------------------------------------

        private struct StarResult
        {
            public int X, Y, Brightness;
        }

        private static StarResult? FindBrightestStar(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var rect   = new Rectangle(0, 0, w, h);
            var data   = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            int bytes  = Math.Abs(stride) * h;
            var buf    = new byte[bytes];
            Marshal.Copy(data.Scan0, buf, 0, bytes);
            bmp.UnlockBits(data);

            // Extract luminance channel (R == lum after ToGrayscale)
            byte[] gray = new byte[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    gray[y * w + x] = buf[y * stride + x * 4 + 2];

            const int nmsHalf   = 5;  // half-size of NMS window
            const int minBright = 50; // brightness floor

            StarResult? best = null;

            for (int y = nmsHalf; y < h - nmsHalf; y++)
            {
                for (int x = nmsHalf; x < w - nmsHalf; x++)
                {
                    byte val = gray[y * w + x];
                    if (val <= minBright) continue;

                    bool isMax = true;
                    for (int ky = -nmsHalf; ky <= nmsHalf && isMax; ky++)
                    {
                        for (int kx = -nmsHalf; kx <= nmsHalf && isMax; kx++)
                        {
                            if (ky == 0 && kx == 0) continue;
                            if (gray[(y + ky) * w + (x + kx)] >= val)
                                isMax = false;
                        }
                    }

                    if (isMax && (best == null || val > best.Value.Brightness))
                        best = new StarResult { X = x, Y = y, Brightness = val };
                }
            }

            return best;
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
