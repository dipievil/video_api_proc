using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class JobStatusResponse
{
    public Guid JobId { get; set; }
    public JobStatus Status { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
    public long? OutputFileSizeBytes { get; set; }
    public List<ProcessingOperationDto> Operations { get; set; } = new();
}

public class ProcessingOperationDto
{
    public string OperationType { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorDetails { get; set; }
}
