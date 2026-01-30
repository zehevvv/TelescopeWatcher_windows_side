using System.Net.Http;
using System.Diagnostics;

namespace TelescopeWatcher
{
    public class MjpegStreamClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private CancellationTokenSource? cancellationToken;
        private Task? streamTask;
        public bool IsStreaming { get; private set; } = false;
        
        public event EventHandler<Image>? FrameReceived;
        public event EventHandler<string>? LogMessage;
        
        // Settings
        public bool FlipHorizontal { get; set; } = true;
        public bool FlipVertical { get; set; } = true;

        public MjpegStreamClient()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task StartStream(string mjpegUrl, int streamId)
        {
            StopStreaming();

            cancellationToken = new CancellationTokenSource();
            IsStreaming = true;
            
            streamTask = Task.Run(async () => 
            {
                try
                {
                    await ProcessStream(mjpegUrl, streamId, cancellationToken.Token);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                     Log($"Stream {streamId} - Error: {ex}");
                }
            }, cancellationToken.Token);

            await Task.CompletedTask;
        }

        private async Task ProcessStream(string mjpegUrl, int streamId, CancellationToken token)
        {
            try
            {
                Log($"Stream {streamId} - URL: {mjpegUrl}");
                
                var request = new HttpRequestMessage(HttpMethod.Get, mjpegUrl);
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

                if (!response.IsSuccessStatusCode)
                {
                    Log($"Stream {streamId} - HTTP Error: {response.StatusCode}");
                    return;
                }

                Log($"Stream {streamId} - Connected successfully");

                using var stream = await response.Content.ReadAsStreamAsync(token);
                
                byte[] buffer = new byte[4096]; // Increased buffer
                List<byte> frameBuffer = new List<byte>();
                
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    for (int i = 0; i < bytesRead; i++)
                    {
                        frameBuffer.Add(buffer[i]);

                        if (frameBuffer.Count < 2) continue;
                        
                        int len = frameBuffer.Count;

                        // Check for JPEG end marker 0xFF 0xD9
                        if (frameBuffer[len - 2] == 0xFF && frameBuffer[len - 1] == 0xD9)
                        {
                            // Find Start Of Image 0xFF 0xD8
                            int startIndex = -1;
                            for (int j = 0; j < frameBuffer.Count - 1; j++)
                            {
                                if (frameBuffer[j] == 0xFF && frameBuffer[j + 1] == 0xD8)
                                {
                                    startIndex = j;
                                    break;
                                }
                            }
                            
                            if (startIndex >= 0)
                            {
                                ProcessFrame(frameBuffer, startIndex, streamId);
                            }
                            
                            frameBuffer.Clear();
                        }
                        
                        // Safety clear if buffer gets too large (no frame found)
                        if (frameBuffer.Count > 1000000) frameBuffer.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                 if (!token.IsCancellationRequested)
                     Log($"Stream {streamId} - Read error: {ex.Message}");
            }
        }

        private void ProcessFrame(List<byte> frameBuffer, int startIndex, int streamId)
        {
            try
            {
                int frameLength = frameBuffer.Count - startIndex;
                byte[] jpegData = new byte[frameLength];
                frameBuffer.CopyTo(startIndex, jpegData, 0, frameLength);
                
                using var ms = new MemoryStream(jpegData);
                var image = Image.FromStream(ms);

                // Handle flipping
                if (FlipHorizontal || FlipVertical)
                {
                    var bitmap = new Bitmap(image);
                    if (FlipHorizontal && FlipVertical) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    else if (FlipHorizontal) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    else if (FlipVertical) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    
                    FrameReceived?.Invoke(this, bitmap);
                    image.Dispose();
                }
                else
                {
                    FrameReceived?.Invoke(this, image);
                }
            }
            catch (Exception ex)
            {
                Log($"Stream {streamId} - Error decoding: {ex.Message}");
            }
        }

        public void StopStreaming()
        {
            IsStreaming = false;
            cancellationToken?.Cancel();

            try
            {
                streamTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch { }
            
            cancellationToken?.Dispose();
            cancellationToken = null;
            streamTask = null;
        }

        public void Dispose()
        {
            StopStreaming();
            httpClient.Dispose();
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            LogMessage?.Invoke(this, message);
        }
    }
}
