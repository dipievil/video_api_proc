namespace VideoProcessingApi.Configuration;

public class FFmpegSettings
{
    public string BinaryPath { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; }
    public string DefaultQuality { get; set; } = string.Empty;
    public Dictionary<string, string> QualityPresets { get; set; } = new();
}
