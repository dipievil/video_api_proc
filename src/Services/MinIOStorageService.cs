using Minio;
using Minio.DataModel.Args;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Interfaces;
using Microsoft.Extensions.Options;

namespace VideoProcessingApi.Services;

public class MinIOStorageService : IStorageService
{
    private readonly MinIOStorageSettings _settings;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinIOStorageService> _logger;

    public MinIOStorageService(
        IOptions<StorageSettings> storageSettings,
        IMinioClient minioClient,
        ILogger<MinIOStorageService> logger)
    {
        _settings = storageSettings.Value.MinIO;
        _minioClient = minioClient;
        _logger = logger;
        
        EnsureBucketExistsAsync().Wait();
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        var fileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
        var objectName = GetObjectName(directory, fileName);

        using var stream = file.OpenReadStream();
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType ?? "application/octet-stream"));

        _logger.LogInformation("File saved to MinIO: {ObjectName}", objectName);
        return objectName;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string directory)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var objectName = GetObjectName(directory, sanitizedFileName);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType("application/octet-stream"));

        _logger.LogInformation("File saved to MinIO: {ObjectName}", objectName);
        return objectName;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath));

            _logger.LogInformation("File deleted from MinIO: {ObjectName}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from MinIO: {ObjectName}", filePath);
            return false;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        try
        {
            var memoryStream = new MemoryStream();
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file from MinIO: {ObjectName}", filePath);
            throw new FileNotFoundException($"File not found in MinIO: {filePath}", ex);
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(filePath));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetFullPath(string directory, string fileName)
    {
        return GetObjectName(directory, fileName);
    }

    public async Task<List<string>> ListFilesAsync(string directory)
    {
        try
        {
            var objects = new List<string>();
            var listArgs = new ListObjectsArgs()
                .WithBucket(_settings.BucketName)
                .WithPrefix(directory.Trim('/') + "/")
                .WithRecursive(true);

            var observable = _minioClient.ListObjectsEnumAsync(listArgs);
            await foreach (var item in observable)
            {
                objects.Add(item.Key);
            }

            return objects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files in MinIO directory: {Directory}", directory);
            throw;
        }
    }
        private async Task EnsureBucketExistsAsync()
    {
        if (!_settings.CreateBucketIfNotExists)
            return;

        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs()
                .WithBucket(_settings.BucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs()
                    .WithBucket(_settings.BucketName));

                _logger.LogInformation("Created MinIO bucket: {BucketName}", _settings.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure bucket exists: {BucketName}", _settings.BucketName);
            throw;
        }
    }

    private static string GetObjectName(string directory, string fileName)
    {
        return $"{directory.Trim('/')}/{fileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}