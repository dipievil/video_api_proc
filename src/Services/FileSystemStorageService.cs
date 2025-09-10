using VideoProcessingApi.Configuration;
using VideoProcessingApi.Interfaces;
using Microsoft.Extensions.Options;

namespace VideoProcessingApi.Services;

public class FileSystemStorageService : IStorageService
{
    private readonly FileSystemStorageSettings _settings;
    private readonly ILogger<FileSystemStorageService> _logger;

    public FileSystemStorageService(
        IOptions<StorageSettings> storageSettings,
        ILogger<FileSystemStorageService> logger)
    {
        _settings = storageSettings.Value.FileSystem;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        var fullDirectory = Path.Combine(_settings.BasePath, directory);
        if (!Directory.Exists(fullDirectory))
        {
            Directory.CreateDirectory(fullDirectory);
        }

        var fileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
        var filePath = Path.Combine(fullDirectory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("File saved to filesystem: {FilePath}", filePath);
        return GetRelativePath(filePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string directory)
    {
        var fullDirectory = Path.Combine(_settings.BasePath, directory);
        if (!Directory.Exists(fullDirectory))
        {
            Directory.CreateDirectory(fullDirectory);
        }

        var sanitizedFileName = SanitizeFileName(fileName);
        var filePath = Path.Combine(fullDirectory, sanitizedFileName);

        using var fileStreamOut = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut);

        _logger.LogInformation("File saved to filesystem: {FilePath}", filePath);
        return GetRelativePath(filePath);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("File deleted from filesystem: {FilePath}", fullPath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from filesystem: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fullPath}");
        }

        return await Task.FromResult(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        return await Task.FromResult(File.Exists(fullPath));
    }

    public string GetFullPath(string directory, string fileName)
    {
        return Path.Combine(_settings.BasePath, directory, fileName);
    }

    private string GetFullPath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }
        return Path.Combine(_settings.BasePath, relativePath);
    }

    private string GetRelativePath(string fullPath)
    {
        var basePath = Path.GetFullPath(_settings.BasePath);
        var fullFilePath = Path.GetFullPath(fullPath);
        
        if (fullFilePath.StartsWith(basePath))
        {
            return Path.GetRelativePath(basePath, fullFilePath);
        }
        
        return fullPath;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}