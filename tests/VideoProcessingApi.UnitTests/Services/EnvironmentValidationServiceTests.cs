using Microsoft.EntityFrameworkCore;
using VideoProcessingApi.Data;
using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.UnitTests.Services;

public class EnvironmentValidationServiceTests : IDisposable
{
    private readonly JobDbContext _context;
    private readonly Mock<IFFmpegService> _mockFFmpegService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<EnvironmentValidationService>> _mockLogger;

    public EnvironmentValidationServiceTests()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<JobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new JobDbContext(options);
        _mockFFmpegService = new Mock<IFFmpegService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<EnvironmentValidationService>>();
    }

    private EnvironmentValidationService CreateService(
        StorageSettings? storageSettings = null, 
        FFmpegSettings? ffmpegSettings = null)
    {
        var defaultStorageSettings = storageSettings ?? new StorageSettings
        {
            Provider = "FileSystem",
            FileSystem = new FileSystemStorageSettings { BasePath = "/tmp/test" }
        };

        var defaultFFmpegSettings = ffmpegSettings ?? new FFmpegSettings
        {
            BinaryPath = "ffmpeg"
        };

        var mockStorageOptions = new Mock<IOptions<StorageSettings>>();
        mockStorageOptions.Setup(x => x.Value).Returns(defaultStorageSettings);

        var mockFFmpegOptions = new Mock<IOptions<FFmpegSettings>>();
        mockFFmpegOptions.Setup(x => x.Value).Returns(defaultFFmpegSettings);

        return new EnvironmentValidationService(
            _context,
            _mockFFmpegService.Object,
            mockStorageOptions.Object,
            mockFFmpegOptions.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ValidateEnvironmentAsync_ShouldReturnValidationResponse()
    {
        // Arrange
        var service = CreateService();
        _mockFFmpegService
            .Setup(x => x.IsFFmpegAvailableAsync())
            .ReturnsAsync(true);

        // Act
        var result = await service.ValidateEnvironmentAsync();

        // Assert
        result.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Database.Should().NotBeNull();
        result.Storage.Should().NotBeNull();
        result.FFmpeg.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateEnvironmentAsync_WithHealthyDatabase_ShouldReturnConnectedStatus()
    {
        // Arrange
        var service = CreateService();
        _mockFFmpegService
            .Setup(x => x.IsFFmpegAvailableAsync())
            .ReturnsAsync(true);

        // Act
        var result = await service.ValidateEnvironmentAsync();

        // Assert
        result.Database.IsAvailable.Should().BeTrue();
        result.Database.Status.Should().Be("Connected");
        result.Database.Provider.Should().Be("SQLite");
        result.Database.Details.Should().ContainKey("PendingMigrations");
        result.Database.Details.Should().ContainKey("DatabaseExists");
    }

    [Fact]
    public async Task ValidateEnvironmentAsync_WithFileSystemProvider_ShouldSetCorrectProvider()
    {
        // Arrange
        var storageSettings = new StorageSettings
        {
            Provider = "FileSystem",
            FileSystem = new FileSystemStorageSettings { BasePath = "/tmp/test-storage" }
        };
        var service = CreateService(storageSettings: storageSettings);

        _mockFFmpegService
            .Setup(x => x.IsFFmpegAvailableAsync())
            .ReturnsAsync(true);

        // Act
        var result = await service.ValidateEnvironmentAsync();

        // Assert
        result.Storage.Provider.Should().Be("FileSystem");
        result.Storage.Configuration.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateEnvironmentAsync_WhenFFmpegThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var service = CreateService();
        _mockFFmpegService
            .Setup(x => x.IsFFmpegAvailableAsync())
            .ThrowsAsync(new InvalidOperationException("FFmpeg test error"));

        // Act
        var result = await service.ValidateEnvironmentAsync();

        // Assert
        result.FFmpeg.IsAvailable.Should().BeFalse();
        result.FFmpeg.Status.Should().Be("Error");
        result.FFmpeg.ErrorMessage.Should().Be("FFmpeg test error");
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateEnvironmentAsync_ShouldSetFFmpegPath()
    {
        // Arrange
        var ffmpegSettings = new FFmpegSettings { BinaryPath = "/usr/bin/ffmpeg" };
        var service = CreateService(ffmpegSettings: ffmpegSettings);
        
        _mockFFmpegService
            .Setup(x => x.IsFFmpegAvailableAsync())
            .ReturnsAsync(true);

        // Act
        var result = await service.ValidateEnvironmentAsync();

        // Assert
        result.FFmpeg.Path.Should().Be("/usr/bin/ffmpeg");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}