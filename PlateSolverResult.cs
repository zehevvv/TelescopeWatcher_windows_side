using System.Text.Json.Serialization;

namespace TelescopeWatcher
{
    public class PlateSolverResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("ra_deg")]
        public double RaDeg { get; set; }

        [JsonPropertyName("dec_deg")]
        public double DecDeg { get; set; }

        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }

        [JsonPropertyName("orientation")]
        public double Orientation { get; set; }

        [JsonPropertyName("pixel_scale")]
        public double PixelScale { get; set; }

        [JsonPropertyName("radius")]
        public double Radius { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
