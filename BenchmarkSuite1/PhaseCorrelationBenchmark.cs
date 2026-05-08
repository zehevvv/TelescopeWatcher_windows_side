using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

[CPUUsageDiagnoser]
public class PhaseCorrelationBenchmark
{
    private Bitmap _reference = null !;
    private Bitmap _current = null !;
    // Matches a typical HD camera frame (coordinates in logs: up to ~1280x720)
    private const int FrameWidth = 1280;
    private const int FrameHeight = 720;
    [GlobalSetup]
    public void Setup()
    {
        _reference = CreateStarMask(FrameWidth, FrameHeight, new[] { (640, 360, 220), (320, 180, 200), (960, 540, 190) });
        _current = CreateStarMask(FrameWidth, FrameHeight, new[] { (643, 363, 220), (323, 183, 200), (963, 543, 190) });
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _reference.Dispose();
        _current.Dispose();
    }

    [Benchmark(Baseline = true)]
    public (int dx, int dy)? FullResolution() => PhaseCorrelation.EstimateOffset(_reference, _current);

    [Benchmark]
    public (int dx, int dy)? Downsampled() => PhaseCorrelation.EstimateOffset(_reference, _current, scale: 0.25f);

    // --- helpers -----------------------------------------------------------
    private static Bitmap CreateStarMask(int w, int h, (int x, int y, int brightness)[] stars)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, w, h);
        var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        int stride = data.Stride;
        var buf = new byte[Math.Abs(stride) * h];
        const int R = 5;
        foreach (var(sx, sy, br)in stars)
        {
            for (int ky = -R; ky <= R; ky++)
            {
                int py = sy + ky;
                if (py < 0 || py >= h)
                    continue;
                for (int kx = -R; kx <= R; kx++)
                {
                    if (kx * kx + ky * ky > R * R)
                        continue;
                    int px = sx + kx;
                    if (px < 0 || px >= w)
                        continue;
                    int idx = py * stride + px * 4;
                    buf[idx] = buf[idx + 1] = buf[idx + 2] = (byte)br;
                    buf[idx + 3] = 255;
                }
            }
        }

        Marshal.Copy(buf, 0, data.Scan0, buf.Length);
        bmp.UnlockBits(data);
        return bmp;
    }
}