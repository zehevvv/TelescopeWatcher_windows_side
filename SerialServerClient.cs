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
            string url = $"{serverUrl}/write?cmd={encodedCmd}";
            
            System.Diagnostics.Debug.WriteLine($"Sending: {url}");
            
            // Fire and forget to avoid blocking UI
            Task.Run(async () =>
            {
                try
                {
                    var response = await commandClient.GetAsync(url);
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
                string url = $"{serverUrl}/read";
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
