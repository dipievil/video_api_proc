using Microsoft.AspNetCore.Mvc;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateJobResponse>> CreateJob([FromForm] CreateJobRequest request)
    {
        try
        {
            var apiKey = HttpContext.Request.Headers["X-API-Key"].FirstOrDefault() ?? "unknown";
            var jobId = await _jobService.CreateJobAsync(request, apiKey);
            
            return CreatedAtAction(
                nameof(GetJobStatus), 
                new { id = jobId }, 
                new CreateJobResponse { JobId = jobId, Message = "Job created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<JobStatusResponse>> GetJobStatus(Guid id)
    {
        var response = await _jobService.GetJobStatusAsync(id);
        
        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadResult(Guid id)
    {
        var stream = await _jobService.GetJobOutputAsync(id);
        
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "video/mp4", $"processed_{id}.mp4");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelJob(Guid id)
    {
        var success = await _jobService.CancelJobAsync(id);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
