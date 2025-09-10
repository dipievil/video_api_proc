using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services;

public class FileService : IFileService
{
    private readonly IStorageService _storageService;
    private readonly ILogger<FileService> _logger;

    public FileService(IStorageService storageService, ILogger<FileService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        return await _storageService.SaveFileAsync(file, directory);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        return await _storageService.DeleteFileAsync(filePath);
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
        return await _storageService.GetFileStreamAsync(filePath);
    }
}
