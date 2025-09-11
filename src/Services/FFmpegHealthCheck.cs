using Microsoft.Extensions.Diagnostics.HealthChecks;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services
{
    public class FFmpegHealthCheck : IHealthCheck
    {
        private readonly IFFmpegService _ffmpegService;
        private readonly ILogger<FFmpegHealthCheck> _logger;

        public FFmpegHealthCheck(IFFmpegService ffmpegService, ILogger<FFmpegHealthCheck> logger)
        {
            _ffmpegService = ffmpegService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = await _ffmpegService.IsFFmpegAvailableAsync();
                
                if (isAvailable)
                {
                    return HealthCheckResult.Healthy("FFmpeg is available and working");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("FFmpeg is not available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking FFmpeg health");
                return HealthCheckResult.Unhealthy("Error checking FFmpeg availability", ex);
            }
        }
    }
}
