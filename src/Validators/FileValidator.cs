using Microsoft.Extensions.Options;
using VideoProcessingApi.Configuration;

namespace VideoProcessingApi.Validators;

public class FileValidator
{
    private readonly ApiSettings _settings;

    public FileValidator(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public bool ValidateFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "File is empty or null";
            return false;
        }

        if (file.Length > _settings.MaxFileSizeBytes)
        {
            errorMessage = $"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes} bytes";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedFileTypes.Contains(extension))
        {
            errorMessage = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedFileTypes)}";
            return false;
        }

        return true;
    }
}
