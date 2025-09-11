using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Data;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;
using Minio;
using System.Diagnostics;

namespace VideoProcessingApi.Services;

public class EnvironmentValidationService : IEnvironmentValidationService
{
    private readonly JobDbContext _dbContext;
    private readonly IFFmpegService _ffmpegService;
    private readonly StorageSettings _storageSettings;
    private readonly FFmpegSettings _ffmpegSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EnvironmentValidationService> _logger;

    public EnvironmentValidationService(
        JobDbContext dbContext,
        IFFmpegService ffmpegService,
        IOptions<StorageSettings> storageSettings,
        IOptions<FFmpegSettings> ffmpegSettings,
        IServiceProvider serviceProvider,
        ILogger<EnvironmentValidationService> logger)
    {
        _dbContext = dbContext;
        _ffmpegService = ffmpegService;
        _storageSettings = storageSettings.Value;
        _ffmpegSettings = ffmpegSettings.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<EnvironmentValidationResponse> ValidateEnvironmentAsync()
    {
        var response = new EnvironmentValidationResponse
        {
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Validate Database
            response.Database = await ValidateDatabaseAsync();

            // Validate Storage
            response.Storage = await ValidateStorageAsync();

            // Validate FFmpeg
            response.FFmpeg = await ValidateFFmpegAsync();

            // Overall health
            response.IsHealthy = response.Database.IsAvailable && 
                               response.Storage.IsAvailable && 
                               response.FFmpeg.IsAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during environment validation");
            response.IsHealthy = false;
        }

        return response;
    }

    private async Task<DatabaseStatus> ValidateDatabaseAsync()
    {
        var status = new DatabaseStatus
        {
            Provider = "SQLite",
            ConnectionString = _dbContext.Database.GetConnectionString() ?? "Not configured"
        };

        try
        {
            await _dbContext.Database.CanConnectAsync();
            status.IsAvailable = true;
            status.Status = "Connected";
            
            // Get additional database info
            var migrations = await _dbContext.Database.GetPendingMigrationsAsync();
            status.Details["PendingMigrations"] = migrations.Count();
            status.Details["DatabaseExists"] = await _dbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database validation failed");
            status.IsAvailable = false;
            status.Status = "Unavailable";
            status.ErrorMessage = ex.Message;
        }

        return status;
    }

    private async Task<StorageStatus> ValidateStorageAsync()
    {
        var status = new StorageStatus
        {
            Provider = _storageSettings.Provider
        };

        try
        {
            if (_storageSettings.Provider.Equals("MinIO", StringComparison.OrdinalIgnoreCase))
            {
                status.Configuration = $"Endpoint: {_storageSettings.MinIO.Endpoint}";
                await ValidateMinIOAsync(status);
            }
            else
            {
                status.Configuration = $"BasePath: {_storageSettings.FileSystem.BasePath}";
                await ValidateFileSystemAsync(status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage validation failed");
            status.IsAvailable = false;
            status.Status = "Unavailable";
            status.ErrorMessage = $"Storage service initialization error: {ex.Message}";
        }

        return status;
    }

    private async Task ValidateMinIOAsync(StorageStatus status)
    {
        try
        {
            var minioClient = _serviceProvider.GetService<IMinioClient>();
            if (minioClient == null)
            {
                status.IsAvailable = false;
                status.Status = "Not configured";
                status.ErrorMessage = "MinIO client not configured";
                return;
            }

            // Check if bucket exists
            var bucketExists = await minioClient.BucketExistsAsync(new Minio.DataModel.Args.BucketExistsArgs()
                .WithBucket(_storageSettings.MinIO.BucketName));
            
            status.IsAvailable = true;
            status.Status = "Connected";
            status.Details["BucketName"] = _storageSettings.MinIO.BucketName;
            status.Details["BucketExists"] = bucketExists;
            status.Details["UseSSL"] = _storageSettings.MinIO.UseSSL;
        }
        catch (Exception ex)
        {
            status.IsAvailable = false;
            status.Status = "Connection failed";
            status.ErrorMessage = ex.Message;
        }
    }

    private async Task ValidateFileSystemAsync(StorageStatus status)
    {
        try
        {
            var basePath = _storageSettings.FileSystem.BasePath;
            var fullPath = Path.GetFullPath(basePath);
            
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            // Test write permission
            var testFile = Path.Combine(fullPath, $"test_{Guid.NewGuid()}.tmp");
            await File.WriteAllTextAsync(testFile, "test");
            File.Delete(testFile);

            status.IsAvailable = true;
            status.Status = "Available";
            status.Details["FullPath"] = fullPath;
            status.Details["Writable"] = true;
        }
        catch (Exception ex)
        {
            status.IsAvailable = false;
            status.Status = "Unavailable";
            status.ErrorMessage = ex.Message;
        }
    }

    private async Task<FFmpegStatus> ValidateFFmpegAsync()
    {
        var status = new FFmpegStatus
        {
            Path = _ffmpegSettings.BinaryPath
        };

        try
        {
            var isAvailable = await _ffmpegService.IsFFmpegAvailableAsync();
            
            if (isAvailable)
            {
                status.IsAvailable = true;
                status.Status = "Available";
                
                // Get FFmpeg version
                await GetFFmpegVersionAsync(status);
            }
            else
            {
                status.IsAvailable = false;
                status.Status = "Not found";
                status.ErrorMessage = "FFmpeg is not available or not found in PATH";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg validation failed");
            status.IsAvailable = false;
            status.Status = "Error";
            status.ErrorMessage = ex.Message;
        }

        return status;
    }

    private async Task GetFFmpegVersionAsync(FFmpegStatus status)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegSettings.BinaryPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                // Extract version from first line
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    status.Version = lines[0].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get FFmpeg version");
            status.Details["VersionError"] = ex.Message;
        }
    }
}