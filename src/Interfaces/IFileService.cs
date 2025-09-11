namespace VideoProcessingApi.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string directory);
    Task<bool> DeleteFileAsync(string filePath);
    bool ValidateFileType(IFormFile file, string[] allowedTypes);
    bool ValidateFileSize(IFormFile file, long maxSizeBytes);
    Task<Stream> GetFileStreamAsync(string filePath);
}
