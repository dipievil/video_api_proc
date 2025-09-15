using VideoProcessingApi.Interfaces;
using VideoProcessingApi.Data;
using Microsoft.EntityFrameworkCore;

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
        // Wait for database to be ready
        await WaitForDatabaseAsync(stoppingToken);
        
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

    private async Task WaitForDatabaseAsync(CancellationToken cancellationToken)
    {
        var maxRetries = 30;
        var retryCount = 0;
        
        while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
                
                await dbContext.Jobs.AnyAsync(cancellationToken);
                
                _logger.LogInformation("Database is ready for cleanup service");
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogDebug(ex, "Database not ready yet (attempt {RetryCount}/{MaxRetries})", retryCount, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
        
        _logger.LogWarning("Database did not become ready within expected time, continuing anyway");
    }
}
