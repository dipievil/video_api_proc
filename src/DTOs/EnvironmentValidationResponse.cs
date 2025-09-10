namespace VideoProcessingApi.DTOs;

public class EnvironmentValidationResponse
{
    public bool IsHealthy { get; set; }
    public DateTime Timestamp { get; set; }
    public DatabaseStatus Database { get; set; } = new();
    public StorageStatus Storage { get; set; } = new();
    public FFmpegStatus FFmpeg { get; set; } = new();
}

public class ComponentStatus
{
    public bool IsAvailable { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public class DatabaseStatus : ComponentStatus
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

public class StorageStatus : ComponentStatus
{
    public string Provider { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
}

public class FFmpegStatus : ComponentStatus
{
    public string? Version { get; set; }
    public string? Path { get; set; }
}