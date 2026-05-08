using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

/// <summary>
/// Estimates the pixel-level translation offset between two same-size bitmaps
/// using phase correlation (normalised cross-power spectrum in the Fourier domain).
///
/// Algorithm:
///   1. Extract luminance channel from each image.
///   2. Zero-pad both to the next power-of-2 size.
///   3. Compute 2-D FFT of each.
///   4. Form the normalised cross-power spectrum: (Fa * conj(Fb)) / |Fa * conj(Fb)|.
///   5. Inverse FFT -> impulse peak at the translation offset.
///   6. Wrap-around: shifts larger than half the padded dimension are negative.
/// </summary>
public static class PhaseCorrelation
{
        /// <summary>
        /// Returns (dx, dy) – how many pixels <paramref name="current"/> has shifted
        /// relative to <paramref name="reference"/>.
        /// Positive dx ? image shifted RIGHT; positive dy ? image shifted DOWN.
        /// Returns null when the images differ in size.
        /// </summary>
        /// <param name="scale">Downsample factor applied before FFT (e.g. 0.25 = quarter resolution).
        /// Reduces FFT size and computation time; the returned offset is automatically scaled back up.</param>
        public static (int dx, int dy)? EstimateOffset(Bitmap reference, Bitmap current, float scale = 1.0f)
        {
            int w = reference.Width, h = reference.Height;
            if (current.Width != w || current.Height != h) return null;

            float[] a = ExtractLuminance(reference, w, h);
            float[] b = ExtractLuminance(current,   w, h);

            // Optionally downsample before FFT to reduce computation time.
            int sw = w, sh = h;
            if (scale > 0f && scale < 1.0f)
            {
                sw = Math.Max(1, (int)(w * scale));
                sh = Math.Max(1, (int)(h * scale));
                a  = DownsampleLuminance(a, w, h, sw, sh);
                b  = DownsampleLuminance(b, w, h, sw, sh);
            }

            // Pad both to the next power-of-2 so the FFT butterfly works cleanly.
            int pw = NextPow2(sw), ph = NextPow2(sh);
            float[] paRe = Pad(a, sw, sh, pw, ph), paIm = new float[pw * ph];
            float[] pbRe = Pad(b, sw, sh, pw, ph), pbIm = new float[pw * ph];

            var fa = FFT2D(paRe, paIm, pw, ph, forward: true);
            var fb = FFT2D(pbRe, pbIm, pw, ph, forward: true);

            // Normalised cross-power spectrum: (Fa * conj(Fb)) / |Fa * conj(Fb)|
            var crossRe = new float[pw * ph];
            var crossIm = new float[pw * ph];
            for (int i = 0; i < pw * ph; i++)
            {
                // conj(Fb) flips the sign of the imaginary part
                float re =  fa[i].re * fb[i].re + fa[i].im * fb[i].im;
                float im =  fa[i].im * fb[i].re - fa[i].re * fb[i].im;
                float mag = MathF.Sqrt(re * re + im * im);
                if (mag > 1e-6f) { crossRe[i] = re / mag; crossIm[i] = im / mag; }
            }

            // Inverse FFT ? correlation surface; peak position = translation offset
            var corr = FFT2D(crossRe, crossIm, pw, ph, forward: false);

            float peak = float.MinValue;
            int pi = 0;
            for (int i = 0; i < pw * ph; i++)
                if (corr[i].re > peak) { peak = corr[i].re; pi = i; }

            int px = pi % pw;
            int py = pi / pw;

            // Wrap: a shift of pw-1 is the same as a shift of -1
            if (px > pw / 2) px -= pw;
            if (py > ph / 2) py -= ph;

            // Scale offset back to original resolution
            if (scale > 0f && scale < 1.0f)
            {
                px = (int)MathF.Round(px / scale);
                py = (int)MathF.Round(py / scale);
            }

            return (px, py);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static float[] ExtractLuminance(Bitmap bmp, int w, int h)
        {
            var rect   = new Rectangle(0, 0, w, h);
            var data   = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            var buf    = new byte[Math.Abs(stride) * h];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            bmp.UnlockBits(data);

            var lum = new float[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    lum[y * w + x] = buf[y * stride + x * 4 + 2]; // R == lum after ToGrayscale
            return lum;
        }

        /// <summary>Copies a w×h block into the top-left corner of a dw×dh zero-filled array.</summary>
        private static float[] Pad(float[] src, int sw, int sh, int dw, int dh)
        {
            var dst = new float[dw * dh];
            for (int y = 0; y < sh; y++)
                Array.Copy(src, y * sw, dst, y * dw, sw);
            return dst;
        }

        /// <summary>Box-filter downsample of a w×h luminance array to dw×dh.</summary>
        private static float[] DownsampleLuminance(float[] src, int sw, int sh, int dw, int dh)
        {
            var dst    = new float[dw * dh];
            float scaleX = sw / (float)dw;
            float scaleY = sh / (float)dh;

            for (int dy = 0; dy < dh; dy++)
            {
                int y0 = (int)(dy       * scaleY);
                int y1 = Math.Min(sh, (int)((dy + 1) * scaleY));
                if (y1 == y0) y1 = y0 + 1;

                for (int dx = 0; dx < dw; dx++)
                {
                    int x0 = (int)(dx       * scaleX);
                    int x1 = Math.Min(sw, (int)((dx + 1) * scaleX));
                    if (x1 == x0) x1 = x0 + 1;

                    float sum = 0f;
                    int   count = 0;
                    for (int y = y0; y < y1; y++)
                        for (int x = x0; x < x1; x++)
                        { sum += src[y * sw + x]; count++; }

                    dst[dy * dw + dx] = count > 0 ? sum / count : 0f;
                }
            }
            return dst;
        }

        private static int NextPow2(int n)
        {
            int p = 1;
            while (p < n) p <<= 1;
            return p;
        }

        // ------------------------------------------------------------------
        // 2-D FFT  (separable row/column Cooley–Tukey, power-of-2 only)
        // ------------------------------------------------------------------

        private static (float re, float im)[] FFT2D(float[] re, float[] im, int w, int h, bool forward)
        {
            var buf = new (float re, float im)[w * h];
            for (int i = 0; i < w * h; i++) buf[i] = (re[i], im[i]);

            var row = new (float re, float im)[w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++) row[x] = buf[y * w + x];
                FFT1D(row, forward);
                for (int x = 0; x < w; x++) buf[y * w + x] = row[x];
            }

            var col = new (float re, float im)[h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++) col[y] = buf[y * w + x];
                FFT1D(col, forward);
                for (int y = 0; y < h; y++) buf[y * w + x] = col[y];
            }

            return buf;
        }

    private static void FFT1D((float re, float im)[] buf, bool forward)
    {
        int n = buf.Length;

        // Bit-reversal permutation
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (buf[i], buf[j]) = (buf[j], buf[i]);
        }

        // Butterfly stages
        for (int len = 2; len <= n; len <<= 1)
        {
            float ang = (forward ? -2f : 2f) * MathF.PI / len;
            (float wRe, float wIm) = (MathF.Cos(ang), MathF.Sin(ang));

            for (int i = 0; i < n; i += len)
            {
                float uRe = 1f, uIm = 0f;
                for (int j = 0; j < len / 2; j++)
                {
                    var u = buf[i + j];
                    var v = buf[i + j + len / 2];
                    float tRe = uRe * v.re - uIm * v.im;
                    float tIm = uRe * v.im + uIm * v.re;
                    buf[i + j]           = (u.re + tRe, u.im + tIm);
                    buf[i + j + len / 2] = (u.re - tRe, u.im - tIm);
                    float nuRe = uRe * wRe - uIm * wIm;
                    uIm  = uRe * wIm + uIm * wRe;
                    uRe  = nuRe;
                }
            }
        }

        // Scale on inverse transform
        if (!forward)
            for (int i = 0; i < n; i++)
                buf[i] = (buf[i].re / n, buf[i].im / n);
    }
}
