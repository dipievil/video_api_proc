namespace VideoProcessingApi.Data.Entities;

public class ProcessingOperation
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorDetails { get; set; }
    
    public VideoJob Job { get; set; } = null!;
}
