namespace VideoProcessingApi.Interfaces;

public interface IFileValidator
{
    bool ValidateFile(IFormFile file, out string errorMessage);
}