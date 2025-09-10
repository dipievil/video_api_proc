using Microsoft.AspNetCore.Mvc;
using VideoProcessingApi.Interfaces;
using VideoProcessingApi.DTOs;

namespace VideoProcessingApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IEnvironmentValidationService _environmentValidationService;

    public HealthController(IEnvironmentValidationService environmentValidationService)
    {
        _environmentValidationService = environmentValidationService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Validates the environment and required assets for the application
    /// </summary>
    /// <returns>Environment validation status including database, storage, and FFmpeg availability</returns>
    [HttpGet("environment")]
    [ProducesResponseType(typeof(EnvironmentValidationResponse), 200)]
    public async Task<IActionResult> ValidateEnvironment()
    {
        var validation = await _environmentValidationService.ValidateEnvironmentAsync();
        return Ok(validation);
    }
}
