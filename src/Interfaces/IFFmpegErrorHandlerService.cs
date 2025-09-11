namespace VideoProcessingApi.Interfaces;

public interface IFFmpegErrorHandlerService
{
    /// <summary>
    /// Map raw ffmpeg stderr to a friendly, actionable message.
    /// Returns null when no mapping is available.
    /// </summary>
    string? MapError(string ffmpegError);
}
