using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.BackgroundServices;

public class CleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupBackgroundService> _logger;

    public CleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                
                await jobService.CleanupExpiredJobsAsync();
                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup background service");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
