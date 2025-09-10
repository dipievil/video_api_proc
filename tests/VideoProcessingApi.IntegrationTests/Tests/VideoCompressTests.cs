using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class VideoCompressTests : IntegrationTestBase
{
    public VideoCompressTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task CreateCompressJob_WithBitrate_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { quality = "low", bitrateKbps = 1000 };

        // Act
        var response = await ApiClient.CreateJobAsync("Compress", videoContent, "test_video.mp4", options);

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
    public async Task CreateCompressJob_HighCompression_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { quality = "low", bitrateKbps = 500 };

        // Act
        var response = await ApiClient.CreateJobAsync("Compress", videoContent, "test_video.mp4", options);

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
    public async Task CreateCompressJob_AndWaitForCompletion_ShouldSucceed()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { quality = "medium", bitrateKbps = 1500 };

        // Act - Create job
        var createResponse = await ApiClient.CreateJobAsync("Compress", videoContent, "test_video.mp4", options);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Wait for completion
        var finalStatus = await ApiClient.WaitForJobCompletionAsync(jobResponse!.JobId, TimeSpan.FromMinutes(3));

        // Assert
        finalStatus.Should().NotBeNull();
        finalStatus.Status.Should().BeOneOf("Completed", "Failed");
        finalStatus.JobId.Should().Be(jobResponse.JobId);

        if (finalStatus.Status == "Failed")
        {
            finalStatus.ErrorMessage.Should().NotBeNullOrEmpty();
            throw new InvalidOperationException($"Compression job failed: {finalStatus.ErrorMessage}");
        }
    }

    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(3000)]
    public async Task CreateCompressJob_WithDifferentBitrates_ShouldReturnSuccess(int bitrateKbps)
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { quality = "medium", bitrateKbps };

        // Act
        var response = await ApiClient.CreateJobAsync("Compress", videoContent, "test_video.mp4", options);

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