using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class VideoConvertTests : IntegrationTestBase
{
    public VideoConvertTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task CreateConvertJob_WithOutputFormat_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "avi", quality = "medium" };

        // Act
        var response = await ApiClient.CreateJobAsync("Convert", videoContent, "test_video.mp4", options);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jobResponse.Should().NotBeNull();
        jobResponse!.JobId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateConvertJob_HighQuality_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "mp4", quality = "high" };

        // Act
        var response = await ApiClient.CreateJobAsync("Convert", videoContent, "test_video.mp4", options);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jobResponse.Should().NotBeNull();
        jobResponse!.JobId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateConvertJob_WithResolution_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "mp4", quality = "medium", resolution = "1280x720" };

        // Act
        var response = await ApiClient.CreateJobAsync("Convert", videoContent, "test_video.mp4", options);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jobResponse.Should().NotBeNull();
        jobResponse!.JobId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("low")]
    [InlineData("medium")]
    [InlineData("high")]
    [InlineData("ultra")]
    public async Task CreateConvertJob_WithDifferentQualities_ShouldReturnSuccess(string quality)
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "mp4", quality };

        // Act
        var response = await ApiClient.CreateJobAsync("Convert", videoContent, "test_video.mp4", options);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jobResponse.Should().NotBeNull();
        jobResponse!.JobId.Should().NotBeEmpty();
    }
}