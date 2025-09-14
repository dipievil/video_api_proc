using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.Interfaces;
using VideoProcessingApi.Services;

namespace VideoProcessingApi.UnitTests.Services;

public class FFmpegServiceVideoInfoTests
{
    private readonly FFmpegService _ffmpegService;
    private readonly Mock<ILogger<FFmpegService>> _mockLogger;
    private readonly Mock<IFFmpegErrorHandlerService> _mockErrorHandler;
    private readonly FFmpegSettings _settings;

    public FFmpegServiceVideoInfoTests()
    {
        _mockLogger = new Mock<ILogger<FFmpegService>>();
        _mockErrorHandler = new Mock<IFFmpegErrorHandlerService>();
        
        _settings = new FFmpegSettings 
        { 
            BinaryPath = "/usr/bin/ffmpeg",
            TimeoutMinutes = 30
        };
        
        var options = Options.Create(_settings);
        _ffmpegService = new FFmpegService(options, _mockLogger.Object, _mockErrorHandler.Object);
    }

    [Fact]
    public void ParseVideoInfo_WithValidJson_ShouldReturnCorrectVideoInfo()
    {
        // Arrange
        var jsonOutput = """
        {
          "streams": [
            {
              "index": 0,
              "codec_name": "h264",
              "codec_type": "video",
              "width": 1920,
              "height": 1080,
              "r_frame_rate": "30/1"
            },
            {
              "index": 1,
              "codec_name": "aac",
              "codec_type": "audio"
            }
          ],
          "format": {
            "filename": "test.mp4",
            "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
            "duration": "120.500000",
            "bit_rate": "2500000"
          }
        }
        """;

        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, "dummy video content");
        
        try
        {
            // Act - usando reflexão para acessar o método privado
            var method = typeof(FFmpegService).GetMethod("ParseVideoInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(_ffmpegService, new object[] { jsonOutput, tempFilePath, "test.mp4" });

            // Assert
            result.Should().NotBeNull();
            var videoInfo = result as VideoProcessingApi.DTOs.VideoInfoResponse;
            videoInfo.Should().NotBeNull();
            videoInfo!.Width.Should().Be(1920);
            videoInfo.Height.Should().Be(1080);
            videoInfo.AspectRatio.Should().Be("16:9");
            videoInfo.Duration.Should().Be(120.5);
            videoInfo.Bitrate.Should().Be(2500); // Converted from bps to kbps
            videoInfo.FrameRate.Should().Be(30.0);
            videoInfo.Codec.Should().Be("h264");
            videoInfo.AudioCodec.Should().Be("aac");
            videoInfo.Filename.Should().Be("test.mp4");
            videoInfo.Format.Should().Be("mov");
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public void CalculateAspectRatio_WithStandardResolutions_ShouldReturnCorrectRatio()
    {
        // Act & Assert using reflection to access private method
        var method = typeof(FFmpegService).GetMethod("CalculateAspectRatio", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var ratio1920x1080 = method?.Invoke(null, new object[] { 1920, 1080 }) as string;
        var ratio1280x720 = method?.Invoke(null, new object[] { 1280, 720 }) as string;
        var ratio800x600 = method?.Invoke(null, new object[] { 800, 600 }) as string;

        ratio1920x1080.Should().Be("16:9");
        ratio1280x720.Should().Be("16:9");
        ratio800x600.Should().Be("4:3");
    }

    [Fact]
    public void ParseFrameRate_WithValidFrameRateString_ShouldReturnCorrectValue()
    {
        // Act & Assert using reflection to access private method
        var method = typeof(FFmpegService).GetMethod("ParseFrameRate", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var frameRate30 = method?.Invoke(null, new object[] { "30/1" });
        var frameRate25 = method?.Invoke(null, new object[] { "25/1" });
        var frameRate2997 = method?.Invoke(null, new object[] { "30000/1001" });

        frameRate30.Should().Be(30.0);
        frameRate25.Should().Be(25.0);
        ((double)frameRate2997!).Should().BeApproximately(29.97, 0.01);
    }

    [Fact]
    public void ParseFrameRate_WithInvalidFrameRateString_ShouldReturnZero()
    {
        // Act & Assert using reflection to access private method
        var method = typeof(FFmpegService).GetMethod("ParseFrameRate", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var frameRateInvalid = method?.Invoke(null, new object[] { "invalid" });
        var frameRateZeroDivision = method?.Invoke(null, new object[] { "30/0" });

        frameRateInvalid.Should().Be(0.0);
        frameRateZeroDivision.Should().Be(0.0);
    }
}