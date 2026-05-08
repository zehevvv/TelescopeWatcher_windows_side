using System.Net.Http;

namespace TelescopeWatcher
{
    public class SerialServerClient
    {
        private readonly string serverUrl;
        private readonly HttpClient commandClient;

        public SerialServerClient(string serverUrl)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            
            this.commandClient = new HttpClient();
            this.commandClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public void WriteLine(string command)
        {
            string encodedCmd = Uri.EscapeDataString(command);
            string url = $"{serverUrl}/motor/write?cmd={encodedCmd}";

            System.Diagnostics.Debug.WriteLine($"Sending: {url}");

            // Fire and forget: do not wait for the response body.
            // ResponseHeadersRead returns as soon as the server sends the status line,
            // and the 500 ms CancellationToken ensures a slow server never holds a
            // connection-pool slot long enough to stall the next tracking iteration.
            _ = Task.Run(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                try
                {
                    using var response = await commandClient.GetAsync(
                        url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                    System.Diagnostics.Debug.WriteLine($"Response: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                }
            });
        }

        public string ReadExisting()
        {
            try
            {
                string url = $"{serverUrl}/motor/read";
                var response = commandClient.GetAsync(url).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result ?? string.Empty;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsConnected()
        {
            return true;
        }

        public void Dispose()
        {
            commandClient?.Dispose();
        }
    }
}
