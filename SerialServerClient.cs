using System.Net.Http;

namespace TelescopeWatcher
{
    public class SerialServerClient
    {
        private readonly string serverUrl;
        private readonly HttpClient httpClient;

        public SerialServerClient(string serverUrl)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            this.httpClient = new HttpClient();
            this.httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public void WriteLine(string command)
        {
            try
            {
                string encodedCmd = Uri.EscapeDataString(command);
                string url = $"{serverUrl}/write?cmd={encodedCmd}";
                
                var response = httpClient.GetAsync(url).Result;
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Server error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send command to server: {ex.Message}", ex);
            }
        }

        public string ReadExisting()
        {
            try
            {
                string url = $"{serverUrl}/read";
                var response = httpClient.GetAsync(url).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new Exception($"Server error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read from server: {ex.Message}", ex);
            }
        }

        public bool IsConnected()
        {
            try
            {
                string url = $"{serverUrl}/read";
                var response = httpClient.GetAsync(url).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Close()
        {
            // Nothing to do for HTTP connection
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
