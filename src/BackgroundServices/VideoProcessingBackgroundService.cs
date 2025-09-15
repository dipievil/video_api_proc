using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Data;
using VideoProcessingApi.Data.Entities;
using VideoProcessingApi.Enums;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.BackgroundServices;

public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public VideoProcessingBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<VideoProcessingBackgroundService> logger,
        IOptions<ApiSettings> apiSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Garantir que MaxConcurrentJobs seja pelo menos 1
        var maxJobs = Math.Max(1, apiSettings.Value.MaxConcurrentJobs);
        _semaphore = new SemaphoreSlim(maxJobs, maxJobs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for database to be ready
        await WaitForDatabaseAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background processing");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task WaitForDatabaseAsync(CancellationToken cancellationToken)
    {
        var maxRetries = 30; // 30 attempts, 2 seconds each = 1 minute max
        var retryCount = 0;
        
        while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
                
                // Try to query the database to check if it's ready
                await dbContext.Jobs.AnyAsync(cancellationToken);
                
                _logger.LogInformation("Database is ready for video processing service");
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

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
        var ffmpegService = scope.ServiceProvider.GetRequiredService<IFFmpegService>();

        var pendingJobs = await dbContext.Jobs
            .Where(j => j.Status == JobStatus.Pending && !j.IsCanceled)
            .OrderBy(j => j.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var tasks = pendingJobs.Select(job => ProcessJobAsync(job, ffmpegService, dbContext, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessJobAsync(VideoJob job, IFFmpegService ffmpegService, JobDbContext dbContext, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Starting processing for job {JobId}", job.Id);
            
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            // Resolve processed directory from Api settings; fallback to ./processed if empty
            var apiSettings = _serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
            var processedDir = string.IsNullOrWhiteSpace(apiSettings.ProcessedPath)
                ? Path.Combine(AppContext.BaseDirectory, "processed")
                : apiSettings.ProcessedPath;

            Directory.CreateDirectory(processedDir);
            var outputPath = Path.Combine(processedDir, $"{job.Id}.mp4");
            
            switch (job.ProcessingType)
            {
                case ProcessingType.Merge:
                    await ffmpegService.MergeVideosAsync(job.InputFilePaths, outputPath, job.Options);
                    break;
                case ProcessingType.Convert:
                {
                    if (!job.InputFilePaths.Any())
                        throw new InvalidOperationException("No input files provided for Convert operation");

                    var input = job.InputFilePaths.First();
                    await ffmpegService.ConvertVideoAsync(input, outputPath, job.Options!);
                    break;
                }
                case ProcessingType.Compress:
                {
                    if (!job.InputFilePaths.Any())
                        throw new InvalidOperationException("No input files provided for Compress operation");

                    var input = job.InputFilePaths.First();
                    await ffmpegService.CompressVideoAsync(input, outputPath, job.Options!);
                    break;
                }
                case ProcessingType.Trim:
                {
                    if (!job.InputFilePaths.Any())
                        throw new InvalidOperationException("No input files provided for Trim operation");

                    if (job.Options?.StartTime == null || job.Options.EndTime == null)
                        throw new InvalidOperationException("Trim operation requires StartTime and EndTime in Options");

                    var input = job.InputFilePaths.First();
                    await ffmpegService.TrimVideoAsync(input, outputPath, job.Options!.StartTime!.Value, job.Options.EndTime!.Value);
                    break;
                }
            }

            job.Status = JobStatus.Completed;
            job.OutputFilePath = outputPath;
            job.FinishedAt = DateTime.UtcNow;
            job.ProcessingDuration = job.FinishedAt - job.StartedAt;
            
            if (File.Exists(outputPath))
            {
                job.OutputFileSizeBytes = new FileInfo(outputPath).Length;
            }

            _logger.LogInformation("Completed processing for job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process job {JobId}", job.Id);
            
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
            job.RetryCount++;
        }
        finally
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _semaphore.Release();
        }
    }
}
