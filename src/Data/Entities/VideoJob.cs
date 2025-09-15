using System.Text.Json;
using VideoProcessingApi.Enums;
using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Data.Entities;

public class VideoJob
{
    public Guid Id { get; set; } 
    public JobStatus Status { get; set; }
    public ProcessingType ProcessingType { get; set; }
    public List<string> InputFilePaths { get; set; } = [];
    public string? OutputFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public ProcessingOptions? Options { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? CreatedBy { get; set; }     public long? OutputFileSizeBytes { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
    public int RetryCount { get; set; } = 0;
    public bool IsCanceled { get; set; } = false;    
    
    public List<ProcessingOperation> Operations { get; set; } = new();
}
