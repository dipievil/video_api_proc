using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class VideoInfoTests : IntegrationTestBase
{
    public VideoInfoTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task GetVideoInfo_WithValidVideo_ShouldReturnVideoInformation()
    {
        // Arrange
        var videoContent = GetTestVideoContent();

        // Act
        var response = await ApiClient.GetVideoInfoAsync(videoContent, "test_video.mp4");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var videoInfo = JsonSerializer.Deserialize<VideoInfoResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        videoInfo.Should().NotBeNull();
        videoInfo!.Width.Should().BeGreaterThan(0);
        videoInfo.Height.Should().BeGreaterThan(0);
        videoInfo.Duration.Should().BeGreaterThan(0);
        videoInfo.Filename.Should().Be("test_video.mp4");
        videoInfo.AspectRatio.Should().NotBeNullOrEmpty();
        videoInfo.Codec.Should().NotBeNullOrEmpty();
        videoInfo.Format.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetVideoInfo_WithInvalidFile_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidContent = System.Text.Encoding.UTF8.GetBytes("This is not a video file");

        // Act
        var response = await ApiClient.GetVideoInfoAsync(invalidContent, "invalid.txt");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}

public class VideoInfoResponse
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string AspectRatio { get; set; } = string.Empty;
    public double Duration { get; set; }
    public int Bitrate { get; set; }
    public double FrameRate { get; set; }
    public string Codec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}