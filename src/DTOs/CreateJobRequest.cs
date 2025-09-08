using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class CreateJobRequest
{
    public ProcessingType ProcessingType { get; set; }
    public ProcessingOptions? Options { get; set; }
    public List<IFormFile> Files { get; set; } = new();
}
