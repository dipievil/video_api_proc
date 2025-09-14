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

    /// <summary>
    /// Create a new video processing job. Upload files as multipart/form-data.
    /// Provide <c>Options</c> as a JSON string in the form field (example below).
    /// </summary>
    /// <remarks>
    /// Example Options JSON: { "Resolution":"1280:720","Quality":"fast","CropWidth":640,"CropHeight":360 }
    /// </remarks>
    /// <param name="request">CreateJobRequest payload sent as form-data</param>
    /// <response code="201">Job created</response>
    /// <response code="400">Invalid input</response>
    [HttpPost]
    public async Task<ActionResult<CreateJobResponse>> CreateJob([FromForm] CreateJobRequest request)
    {
        try
        {
            // Ensure uploaded files are available on the request DTO.
            // Some clients may post files but model binding doesn't populate the DTO.Files property.
            if ((request.Files == null || !request.Files.Any()) && Request?.Form?.Files != null && Request.Form.Files.Any())
            {
                request.Files = Request.Form.Files.ToList();
            }

            // Validate that at least one file was uploaded
            var hasFiles = (request.Files != null && request.Files.Any());
            if (!hasFiles)
            {
                return BadRequest(new { error = "Missing file" });
            }

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

    /// <summary>
    /// Get status for a previously created job.
    /// </summary>
    /// <param name="id">Job identifier</param>
    /// <response code="200">Job status returned</response>
    /// <response code="404">Job not found</response>
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

    /// <summary>
    /// Download the processed output for a completed job.
    /// </summary>
    /// <param name="id">Job identifier</param>
    /// <response code="200">File stream returned</response>
    /// <response code="404">File not found or job not completed</response>
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

    /// <summary>
    /// Cancel a pending job. Only pending jobs can be canceled.
    /// </summary>
    /// <param name="id">Job identifier</param>
    /// <response code="204">Job canceled</response>
    /// <response code="404">Job not found or cannot be canceled</response>
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
