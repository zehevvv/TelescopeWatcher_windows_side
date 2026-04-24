using System.Text.Json.Serialization;

namespace TelescopeWatcher
{
    public class CameraInfo
    {
        [JsonPropertyName("camera_id")]
        public string CameraId { get; set; } = "";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        public override string ToString() => Model;
    }

    public class CameraStartResponse
    {
        [JsonPropertyName("camera_id")]
        public string CameraId { get; set; } = "";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("scheme")]
        public string Scheme { get; set; } = "http";

        [JsonPropertyName("stream_port")]
        public int StreamPort { get; set; }

        [JsonPropertyName("stream_path")]
        public string StreamPath { get; set; } = "";

        [JsonPropertyName("manager_port")]
        public int ManagerPort { get; set; }
    }
}
