using System.Net.Http;

namespace TelescopeWatcher
{
    public class SerialServerClient
    {
        private readonly string serverUrl;
        private readonly HttpClient httpClient;
        private Task? streamReadTask;
        private CancellationTokenSource? streamCancellationToken;
        private Action<string>? onDataReceived;

        public SerialServerClient(string serverUrl)
        {
            this.serverUrl = serverUrl.TrimEnd('/');
            this.httpClient = new HttpClient();
            this.httpClient.Timeout = TimeSpan.FromMinutes(30);
        }

        public void StartStreaming(Action<string> onDataReceived)
        {
            this.onDataReceived = onDataReceived;
            streamCancellationToken = new CancellationTokenSource();
            
            streamReadTask = Task.Run(async () =>
            {
                while (!streamCancellationToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, $"{serverUrl}/stream");
                        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, streamCancellationToken.Token);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            using var stream = await response.Content.ReadAsStreamAsync();
                            using var reader = new StreamReader(stream);
                            
                            while (!reader.EndOfStream && !streamCancellationToken.Token.IsCancellationRequested)
                            {
                                var line = await reader.ReadLineAsync();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    onDataReceived?.Invoke(line);
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (!streamCancellationToken.Token.IsCancellationRequested)
                        {
                            await Task.Delay(1000, streamCancellationToken.Token);
                        }
                    }
                }
            }, streamCancellationToken.Token);
        }

        public void StopStreaming()
        {
            streamCancellationToken?.Cancel();
            try { streamReadTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
            streamCancellationToken?.Dispose();
            streamCancellationToken = null;
            streamReadTask = null;
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

        public void Close() => StopStreaming();

        public void Dispose()
        {
            StopStreaming();
            httpClient?.Dispose();
        }
    }
}
