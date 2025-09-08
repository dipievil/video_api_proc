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

            var outputPath = Path.Combine("processed", $"{job.Id}.mp4");
            
            switch (job.ProcessingType)
            {
                case ProcessingType.Merge:
                    await ffmpegService.MergeVideosAsync(job.InputFilePaths, outputPath, job.Options);
                    break;
                case ProcessingType.Convert:
                    await ffmpegService.ConvertVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!);
                    break;
                case ProcessingType.Compress:
                    await ffmpegService.CompressVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!);
                    break;
                case ProcessingType.Trim:
                    await ffmpegService.TrimVideoAsync(job.InputFilePaths.First(), outputPath, job.Options!.StartTime!.Value, job.Options.EndTime!.Value);
                    break;
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
