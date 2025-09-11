using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Interfaces;

public interface IEnvironmentValidationService
{
    Task<EnvironmentValidationResponse> ValidateEnvironmentAsync();
}