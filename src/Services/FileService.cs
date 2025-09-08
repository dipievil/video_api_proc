using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
        var filePath = Path.Combine(directory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return filePath;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return false;
        }
    }

    public bool ValidateFileType(IFormFile file, string[] allowedTypes)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedTypes.Contains(extension);
    }

    public bool ValidateFileSize(IFormFile file, long maxSizeBytes)
    {
        return file.Length <= maxSizeBytes;
    }

    public async Task<Stream> GetFileStreamAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return await Task.FromResult(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}
