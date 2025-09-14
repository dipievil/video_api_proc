using Microsoft.AspNetCore.Mvc;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IFFmpegService _ffmpegService;
    private readonly IFileService _fileService;
    private readonly IFileValidator _fileValidator;
    private readonly ILogger<VideosController> _logger;

    public VideosController(
        IFFmpegService ffmpegService,
        IFileService fileService,
        IFileValidator fileValidator,
        ILogger<VideosController> logger)
    {
        _ffmpegService = ffmpegService;
        _fileService = fileService;
        _fileValidator = fileValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get video information including dimensions, duration, codecs, bitrate, etc.
    /// Upload a single video file to get its metadata information.
    /// </summary>
    /// <param name="file">Video file to analyze</param>
    /// <response code="200">Video information returned</response>
    /// <response code="400">Invalid file or file validation failed</response>
    /// <response code="500">Error processing video file</response>
    [HttpPost("info")]
    public async Task<ActionResult<VideoInfoResponse>> GetVideoInfo(IFormFile file)
    {
        try
        {
            // Validate file
            if (!_fileValidator.ValidateFile(file, out var errorMessage))
            {
                _logger.LogWarning("File validation failed: {ErrorMessage}", errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            // Save file temporarily
            var tempFilePath = await _fileService.SaveTempFileAsync(file);
            
            try
            {
                // Get video information using FFprobe
                var videoInfo = await _ffmpegService.GetVideoInfoAsync(tempFilePath, file.FileName);
                
                _logger.LogInformation("Successfully extracted video info for file: {Filename}", file.FileName);
                
                return Ok(videoInfo);
            }
            finally
            {
                // Clean up temporary file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get video info for file: {Filename}", file?.FileName);
            return StatusCode(500, new { error = "Error processing video file", details = ex.Message });
        }
    }
}