namespace VideoProcessingApi.Configuration;

public class ApiSettings
{
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    public long MaxFileSizeBytes { get; set; }
    public int MaxConcurrentJobs { get; set; }
    public string UploadsPath { get; set; } = string.Empty;
    public string ProcessedPath { get; set; } = string.Empty;
    public TimeSpan FileRetentionPeriod { get; set; }
}
