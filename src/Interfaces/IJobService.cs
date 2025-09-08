using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Interfaces;

public interface IJobService
{
    Task<Guid> CreateJobAsync(CreateJobRequest request, string apiKey);
    Task<JobStatusResponse?> GetJobStatusAsync(Guid jobId);
    Task<bool> CancelJobAsync(Guid jobId);
    Task<Stream?> GetJobOutputAsync(Guid jobId);
    Task CleanupExpiredJobsAsync();
}