using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Data;
using VideoProcessingApi.Data.Entities;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Enums;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services;

public class JobService : IJobService
{
    private readonly JobDbContext _context;
    private readonly IFileService _fileService;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<JobService> _logger;

    public JobService(
        JobDbContext context, 
        IFileService fileService, 
        IOptions<ApiSettings> apiSettings,
        ILogger<JobService> logger)
    {
        _context = context;
        _fileService = fileService;
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public async Task<Guid> CreateJobAsync(CreateJobRequest request, string apiKey)
    {
        var job = new VideoJob
        {
            Id = Guid.NewGuid(),
            ProcessingType = request.ProcessingType,
            Options = request.Options,
            Status = JobStatus.Pending,
            CreatedBy = apiKey,
            InputFilePaths = new List<string>()
        };

        // Save uploaded files
        foreach (var file in request.Files)
        {
            var filePath = await _fileService.SaveFileAsync(file, _apiSettings.UploadsPath);
            job.InputFilePaths.Add(filePath);
        }

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created job {JobId} with {FileCount} files", job.Id, request.Files.Count);
        return job.Id;
    }

    public async Task<JobStatusResponse?> GetJobStatusAsync(Guid jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.Operations)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
        {
            return null;
        }

        return new JobStatusResponse
        {
            JobId = job.Id,
            Status = job.Status,
            DownloadUrl = job.Status == JobStatus.Completed && !string.IsNullOrEmpty(job.OutputFilePath) 
                ? $"/api/jobs/{job.Id}/download" 
                : null,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            FinishedAt = job.FinishedAt,
            ProcessingDuration = job.ProcessingDuration,
            OutputFileSizeBytes = job.OutputFileSizeBytes,
            Operations = job.Operations.Select(op => new ProcessingOperationDto
            {
                OperationType = op.OperationType,
                StartedAt = op.StartedAt,
                CompletedAt = op.CompletedAt,
                IsSuccessful = op.IsSuccessful,
                ErrorDetails = op.ErrorDetails
            }).ToList()
        };
    }

    public async Task<bool> CancelJobAsync(Guid jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        
        if (job == null || job.Status != JobStatus.Pending)
        {
            return false;
        }

        job.IsCanceled = true;
        job.Status = JobStatus.Canceled;
        job.FinishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Job {JobId} was canceled", jobId);
        return true;
    }

    public async Task<Stream?> GetJobOutputAsync(Guid jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        
        if (job?.Status != JobStatus.Completed || string.IsNullOrEmpty(job.OutputFilePath))
        {
            return null;
        }

        return await _fileService.GetFileStreamAsync(job.OutputFilePath);
    }

    public async Task CleanupExpiredJobsAsync()
    {
        var cutoffDate = DateTime.UtcNow.Subtract(_apiSettings.FileRetentionPeriod);
        var expiredJobs = await _context.Jobs
            .Where(j => j.CreatedAt < cutoffDate)
            .ToListAsync();

        foreach (var job in expiredJobs)
        {
            // Delete input files
            foreach (var inputFile in job.InputFilePaths)
            {
                await _fileService.DeleteFileAsync(inputFile);
            }

            // Delete output file
            if (!string.IsNullOrEmpty(job.OutputFilePath))
            {
                await _fileService.DeleteFileAsync(job.OutputFilePath);
            }
        }

        _context.Jobs.RemoveRange(expiredJobs);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} expired jobs", expiredJobs.Count);
    }
}
