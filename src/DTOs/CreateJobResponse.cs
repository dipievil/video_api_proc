namespace VideoProcessingApi.DTOs;

public class CreateJobResponse
{
    public Guid JobId { get; set; }
    public string Message { get; set; } = string.Empty;
}
