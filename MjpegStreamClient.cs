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

        private int _frameCount;
        private long _fpsTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


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

                string boundary = null;
                var contentType = response.Content.Headers.ContentType;
                if (contentType != null && !string.IsNullOrEmpty(contentType.MediaType))
                {
                    if (contentType.MediaType.Contains("multipart"))
                    {
                        foreach (var param in contentType.Parameters)
                        {
                            if (param.Name == "boundary")
                            {
                                boundary = param.Value?.Trim('\"');
                                break;
                            }
                        }
                    }
                }

                // If no boundary found in header, assume typical mjpg-streamer boundary
                if (string.IsNullOrEmpty(boundary))
                {
                    boundary = "--boundarydonotcross"; 
                }
                
                // Ensure boundary starts with --
                if (!boundary.StartsWith("--")) boundary = "--" + boundary;

                Log($"Stream {streamId} - Connected. Boundary: {boundary}");

                using var networkStream = await response.Content.ReadAsStreamAsync(token);
                using var stream = new BufferedStream(networkStream, 1024 * 64); // 64KB Buffer
                
                byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes(boundary);
                
                // Buffer for reading headers and fallback scanning
                List<byte> buffer = new List<byte>();
                byte[] readChunk = new byte[8192]; 

                while (!token.IsCancellationRequested)
                {
                    // 1. Read Headers
                    string headers = "";
                    while (true)
                    {
                        string? line = ReadLine(stream); // Changed to blocking read from BufferedStream for speed
                        if (line == null) return; // End of stream

                        // Skip empty lines that often appear between frames (e.g. trailing CRLF)
                        if (string.IsNullOrEmpty(line))
                        {
                            if (headers.Length == 0) continue; 
                            break; // End of headers block
                        }

                        headers += line + "\n";
                    }

                    // 2. Parse Content-Length
                    int contentLength = -1;
                    foreach(var line in headers.Split('\n'))
                    {
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        {
                            string val = line.Substring(15).Trim();
                            if (int.TryParse(val, out int len))
                            {
                                contentLength = len;
                                break;
                            }
                        }
                    }

                    if (contentLength > 0)
                    {
                        // 3a. Known Length: Read exact content
                        byte[] frameData = new byte[contentLength];
                        int totalRead = 0;
                        while (totalRead < contentLength)
                        {
                            // Using blocking Read on BufferedStream is efficient and avoids async overhead for small chunks
                            int read = stream.Read(frameData, totalRead, contentLength - totalRead);
                            if (read == 0) return;
                            totalRead += read;
                        }
                        
                        ProcessFrameBytes(frameData, streamId);
                    }
                    else
                    {
                        // 3b. Unknown Length: Read until next boundary
                        buffer.Clear();
                        
                        while (!token.IsCancellationRequested)
                        {
                             // Read larger chunks
                            int read = stream.Read(readChunk, 0, readChunk.Length);
                            if (read == 0) return;

                            for (int i = 0; i < read; i++)
                            {
                                buffer.Add(readChunk[i]);
                                
                                // Check for boundary based on last byte matching last boundary byte
                                if (buffer.Count >= boundaryBytes.Length)
                                {
                                    if (buffer[buffer.Count - 1] == boundaryBytes[boundaryBytes.Length - 1])
                                    {
                                        // Potential match, check backwards
                                        bool match = true;
                                        for (int j = 0; j < boundaryBytes.Length; j++)
                                        {
                                            if (buffer[buffer.Count - 1 - j] != boundaryBytes[boundaryBytes.Length - 1 - j])
                                            {
                                                match = false;
                                                break;
                                            }
                                        }

                                        if (match)
                                        {
                                            // Found boundary!
                                            // Extract frame data (everything before the boundary)
                                            // Remove boundary from buffer
                                            int frameSize = buffer.Count - boundaryBytes.Length;
                                            
                                            // There might be a CRLF before boundary, clean it up
                                            if (frameSize > 0 && buffer[frameSize - 1] == '\n') frameSize--;
                                            if (frameSize > 0 && buffer[frameSize - 1] == '\r') frameSize--;

                                            if (frameSize > 0)
                                            {
                                                byte[] frameData = new byte[frameSize];
                                                buffer.CopyTo(0, frameData, 0, frameSize);
                                                ProcessFrameBytes(frameData, streamId);
                                            }
                                            
                                            // Stop reading for this frame, next loop will read headers
                                            goto NextFrame; 
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    NextFrame: continue;
                }
            }
            catch (Exception ex)
            {
                 if (!token.IsCancellationRequested)
                     Log($"Stream {streamId} - Read error: {ex.Message}");
            }
        }

        // Replaced async ReadLineAsync with sync ReadLine since we are in a Task.Run and using BufferedStream
        // Pre-allocated buffer to avoid repeated allocations
        private readonly byte[] _lineBuffer = new byte[512];

        private string? ReadLine(Stream stream)
        {
            int index = 0;
            int b;
            while (true)
            {
                b = stream.ReadByte();
                if (b == -1) return null;

                if (b == '\n')
                {
                    return System.Text.Encoding.ASCII.GetString(_lineBuffer, 0, index).Trim();
                }
                else if (b != '\r' && index < _lineBuffer.Length)
                {
                    _lineBuffer[index++] = (byte)b;
                }
            }
        }




        private void ProcessFrameBytes(byte[] jpegData, int streamId)
        {

            try
            {
                // Validate it's a JPEG
                if (jpegData.Length < 2 || jpegData[0] != 0xFF || jpegData[1] != 0xD8)
                {
                    Log($"it's not jpeg {jpegData[0]}, {jpegData[1]}");
                    return;
                }

                using var ms = new MemoryStream(jpegData);

                // Load directly as Bitmap to avoid double allocation when flipping
                var bitmap = new Bitmap(ms);

                // Handle flipping in-place (no copy needed)
                if (FlipHorizontal && FlipVertical)
                {
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                }
                else if (FlipHorizontal)
                {
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                else if (FlipVertical)
                {
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }



                FrameReceived?.Invoke(this, bitmap);
            }
            catch 
            {
                Log("Corrupt image");
                // Ignore corrupt frames
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
