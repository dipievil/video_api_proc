using Microsoft.AspNetCore.Mvc;
using VideoProcessingApi.Interfaces;

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

    [HttpGet("environment")]
    public async Task<IActionResult> ValidateEnvironment()
    {
        var validation = await _environmentValidationService.ValidateEnvironmentAsync();
        return Ok(validation);
    }
}
