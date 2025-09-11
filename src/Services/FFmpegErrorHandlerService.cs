using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services;

public class FFmpegErrorHandlerService : IFFmpegErrorHandlerService
{
    public string? MapError(string ffmpegError)
    {
        if (string.IsNullOrWhiteSpace(ffmpegError)) return null;

        var lower = ffmpegError.ToLowerInvariant();

        // common concat filter failures due to resolution/aspect mismatch
        if (lower.Contains("failed to configure output pad") || lower.Contains("input link") && lower.Contains("do not match"))
        {
            return FFmpegErrorMessages.VideoSourcesMismatch;
        }

        // generic detection for "Error reinitializing filters" which often indicates filter graph issues
        if (lower.Contains("error reinitializing filters") || lower.Contains("failed to inject frame into filter network"))
        {
            return FFmpegErrorMessages.VideoSourcesMismatch;
        }

        return null;
    }
}
