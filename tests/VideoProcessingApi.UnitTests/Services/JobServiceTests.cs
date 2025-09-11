using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VideoProcessingApi.Data;
using VideoProcessingApi.Data.Entities;

namespace VideoProcessingApi.UnitTests.Services;

public class JobServiceTests : IDisposable
{
    private readonly JobDbContext _context;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IOptions<ApiSettings>> _mockApiSettings;
    private readonly Mock<ILogger<JobService>> _mockLogger;
    private readonly JobService _jobService;

    public JobServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<JobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new JobDbContext(options);
        _mockFileService = new Mock<IFileService>();
        _mockApiSettings = new Mock<IOptions<ApiSettings>>();
        _mockLogger = new Mock<ILogger<JobService>>();

        var apiSettings = new ApiSettings
        {
            UploadsPath = "/uploads"
        };
        _mockApiSettings.Setup(x => x.Value).Returns(apiSettings);

        _jobService = new JobService(_context, _mockFileService.Object, _mockApiSettings.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldCreateJobSuccessfully()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns("test.mp4");
        
        var request = new CreateJobRequest
        {
            ProcessingType = ProcessingType.Convert,
            Options = new ProcessingOptions { OutputFormat = "mp4" },
            Files = new List<IFormFile> { mockFile.Object }
        };

        var apiKey = "test-api-key";
        var expectedFilePath = "/uploads/test.mp4";

        _mockFileService
            .Setup(x => x.SaveFileAsync(mockFile.Object, "/uploads"))
            .ReturnsAsync(expectedFilePath);

        // Act
        var jobId = await _jobService.CreateJobAsync(request, apiKey);

        // Assert
        jobId.Should().NotBeEmpty();
        
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        job.Should().NotBeNull();
        job!.ProcessingType.Should().Be(ProcessingType.Convert);
        job.Status.Should().Be(JobStatus.Pending);
        job.CreatedBy.Should().Be(apiKey);
        job.InputFilePaths.Should().ContainSingle();
        job.InputFilePaths[0].Should().Be(expectedFilePath);
        job.Options.Should().NotBeNull();
        job.Options!.OutputFormat.Should().Be("mp4");

        _mockFileService.Verify(x => x.SaveFileAsync(mockFile.Object, "/uploads"), Times.Once);
    }

    [Fact]
    public async Task GetJobStatusAsync_WithExistingJob_ShouldReturnJobStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new VideoJob
        {
            Id = jobId,
            Status = JobStatus.Processing,
            ProcessingType = ProcessingType.Merge,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            StartedAt = DateTime.UtcNow.AddMinutes(-2),
            CreatedBy = "test-key"
        };

        await _context.Jobs.AddAsync(job);
        await _context.SaveChangesAsync();

        // Act
        var result = await _jobService.GetJobStatusAsync(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobId);
        result.Status.Should().Be(JobStatus.Processing);
        result.CreatedAt.Should().BeCloseTo(job.CreatedAt, TimeSpan.FromSeconds(1));
        result.StartedAt.Should().BeCloseTo(job.StartedAt!.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetJobStatusAsync_WithNonExistentJob_ShouldReturnNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var result = await _jobService.GetJobStatusAsync(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CancelJobAsync_WithPendingJob_ShouldCancelSuccessfully()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new VideoJob
        {
            Id = jobId,
            Status = JobStatus.Pending,
            ProcessingType = ProcessingType.Convert,
            CreatedBy = "test-key"
        };

        await _context.Jobs.AddAsync(job);
        await _context.SaveChangesAsync();

        // Act
        var result = await _jobService.CancelJobAsync(jobId);

        // Assert
        result.Should().BeTrue();
        
        var updatedJob = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        updatedJob!.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task CancelJobAsync_WithNonExistentJob_ShouldReturnFalse()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var result = await _jobService.CancelJobAsync(jobId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobOutputAsync_WithCompletedJobAndOutputFile_ShouldReturnStream()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var outputPath = "/processed/output.mp4";
        var expectedStream = new MemoryStream();
        
        var job = new VideoJob
        {
            Id = jobId,
            Status = JobStatus.Completed,
            ProcessingType = ProcessingType.Convert,
            OutputFilePath = outputPath,
            CreatedBy = "test-key"
        };

        await _context.Jobs.AddAsync(job);
        await _context.SaveChangesAsync();

        _mockFileService
            .Setup(x => x.GetFileStreamAsync(outputPath))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _jobService.GetJobOutputAsync(jobId);

        // Assert
        result.Should().BeSameAs(expectedStream);
        _mockFileService.Verify(x => x.GetFileStreamAsync(outputPath), Times.Once);
    }

    [Fact]
    public async Task GetJobOutputAsync_WithNonExistentJob_ShouldReturnNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var result = await _jobService.GetJobOutputAsync(jobId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}