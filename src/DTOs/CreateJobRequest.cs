using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class CreateJobRequest
{
    /// <summary>
    /// Type of processing to perform on uploaded file(s). Examples: Merge, Convert, Compress, Trim, ExtractAudio.
    /// </summary>
    /// <example>Convert</example>
    public ProcessingType ProcessingType { get; set; }
    /// <summary>
    /// Optional processing options. Provide as JSON when sending multipart/form-data.
    /// Example: { "Resolution":"1280:720","CropWidth":640,"CropHeight":360 }
    /// </summary>
    public ProcessingOptions? Options { get; set; }
    /// <summary>
    /// Files to be processed. Use multipart/form-data to upload one or more files in the "Files" field.
    /// </summary>
    public List<IFormFile> Files { get; set; } = new();
}
