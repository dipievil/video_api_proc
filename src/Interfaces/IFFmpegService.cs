using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Interfaces;

public interface IFFmpegService
{
    Task<string> MergeVideosAsync(List<string> inputPaths, string outputPath, ProcessingOptions? options = null);
    Task<string> ConvertVideoAsync(string inputPath, string outputPath, ProcessingOptions options);
    Task<string> CompressVideoAsync(string inputPath, string outputPath, ProcessingOptions options);
    Task<string> TrimVideoAsync(string inputPath, string outputPath, double startTime, double endTime);
    Task<string> ExtractAudioAsync(string inputPath, string outputPath, ProcessingOptions? options = null);
    Task<bool> ValidateVideoFileAsync(string filePath);
    Task<bool> IsFFmpegAvailableAsync();
}