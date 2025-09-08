namespace VideoProcessingApi.Configuration;

public class SecuritySettings
{
    public string[] ApiKeys { get; set; } = Array.Empty<string>();
    public RateLimitSettings RateLimit { get; set; } = new();
    public CorsSettings Cors { get; set; } = new();
}

public class RateLimitSettings
{
    public int MaxRequestsPerMinute { get; set; }
    public int MaxRequestsPerHour { get; set; }
    public int MaxRequestsPerDay { get; set; }
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
}
