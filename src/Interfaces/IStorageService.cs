namespace VideoProcessingApi.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string directory);
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string directory);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream> GetFileStreamAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string GetFullPath(string directory, string fileName);
}