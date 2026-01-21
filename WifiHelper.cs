using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TelescopeWatcher
{
    public static class WifiHelper
    {
        public static async Task<string?> GetCurrentSSIDAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show interfaces",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Parse output for SSID
                // Line looks like: "    SSID                   : RaspberryPiCam"
                var ssidLine = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));

                if (ssidLine != null)
                {
                    var parts = ssidLine.Split(new[] { ':' }, 2);
                    if (parts.Length > 1)
                    {
                        return parts[1].Trim();
                    }
                }
            }
            catch (Exception)
            {
                // Log or handle error if needed
            }
            return null;
        }

        public static async Task<bool> ConnectToWifiAsync(string ssid)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"wlan connect name=\"{ssid}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output.Contains("Connection request was completed successfully");
            }
            catch
            {
                return false;
            }
        }
    }
}
