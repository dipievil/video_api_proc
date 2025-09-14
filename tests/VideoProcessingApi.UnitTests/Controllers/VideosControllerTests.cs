using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VideoProcessingApi.Controllers;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.UnitTests.Controllers;

public class VideosControllerTests
{
    private readonly VideosController _controller;
    private readonly Mock<IFFmpegService> _mockFFmpegService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IFileValidator> _mockFileValidator;
    private readonly Mock<ILogger<VideosController>> _mockLogger;

    public VideosControllerTests()
    {
        _mockFFmpegService = new Mock<IFFmpegService>();
        _mockFileService = new Mock<IFileService>();
        _mockFileValidator = new Mock<IFileValidator>();
        _mockLogger = new Mock<ILogger<VideosController>>();

        _controller = new VideosController(
            _mockFFmpegService.Object,
            _mockFileService.Object,
            _mockFileValidator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetVideoInfo_WithValidFile_ShouldReturnVideoInfo()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns("test.mp4");
        mockFile.Setup(x => x.Length).Returns(1024);

        var tempFilePath = "/tmp/test-video.mp4";
        var expectedVideoInfo = new VideoInfoResponse
        {
            Width = 1920,
            Height = 1080,
            AspectRatio = "16:9",
            Duration = 120.5,
            Bitrate = 2500,
            FrameRate = 30.0,
            Codec = "h264",
            AudioCodec = "aac",
            FileSize = 1024,
            Filename = "test.mp4",
            Format = "mp4"
        };

        _mockFileValidator
            .Setup(x => x.ValidateFile(mockFile.Object, out It.Ref<string>.IsAny))
            .Returns(true);

        _mockFileService
            .Setup(x => x.SaveTempFileAsync(mockFile.Object))
            .ReturnsAsync(tempFilePath);

        _mockFFmpegService
            .Setup(x => x.GetVideoInfoAsync(tempFilePath, "test.mp4"))
            .ReturnsAsync(expectedVideoInfo);

        // Act
        var result = await _controller.GetVideoInfo(mockFile.Object);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var videoInfo = okResult.Value as VideoInfoResponse;
        videoInfo.Should().NotBeNull();
        videoInfo!.Width.Should().Be(1920);
        videoInfo.Height.Should().Be(1080);
        videoInfo.Filename.Should().Be("test.mp4");
    }

    [Fact]
    public async Task GetVideoInfo_WithInvalidFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns("test.txt");

        var errorMessage = "File type not supported";
        _mockFileValidator
            .Setup(x => x.ValidateFile(mockFile.Object, out errorMessage))
            .Returns(false);

        // Act
        var result = await _controller.GetVideoInfo(mockFile.Object);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetVideoInfo_WhenFFmpegServiceThrows_ShouldReturnInternalServerError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns("test.mp4");

        var tempFilePath = "/tmp/test-video.mp4";

        _mockFileValidator
            .Setup(x => x.ValidateFile(mockFile.Object, out It.Ref<string>.IsAny))
            .Returns(true);

        _mockFileService
            .Setup(x => x.SaveTempFileAsync(mockFile.Object))
            .ReturnsAsync(tempFilePath);

        _mockFFmpegService
            .Setup(x => x.GetVideoInfoAsync(tempFilePath, "test.mp4"))
            .ThrowsAsync(new InvalidOperationException("FFprobe failed"));

        // Act
        var result = await _controller.GetVideoInfo(mockFile.Object);

        // Assert
        result.Should().NotBeNull();
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }
}