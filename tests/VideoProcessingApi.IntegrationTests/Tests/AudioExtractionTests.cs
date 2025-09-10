using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class AudioExtractionTests : IntegrationTestBase
{
    public AudioExtractionTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task CreateExtractAudioJob_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();

        // Act
        var response = await ApiClient.CreateJobAsync("ExtractAudio", videoContent);

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
    public async Task CreateExtractAudioJob_WithFormat_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "mp3" };

        // Act
        var response = await ApiClient.CreateJobAsync("ExtractAudio", videoContent, "test_video.mp4", options);

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
    public async Task CreateExtractAudioJob_AndWaitForCompletion_ShouldSucceed()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat = "mp3", quality = "medium" };

        // Act - Create job
        var createResponse = await ApiClient.CreateJobAsync("ExtractAudio", videoContent, "test_video.mp4", options);
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
            throw new InvalidOperationException($"Audio extraction job failed: {finalStatus.ErrorMessage}");
        }
    }

    [Theory]
    [InlineData("mp3")]
    [InlineData("wav")]
    [InlineData("aac")]
    public async Task CreateExtractAudioJob_WithDifferentFormats_ShouldReturnSuccess(string outputFormat)
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { outputFormat };

        // Act
        var response = await ApiClient.CreateJobAsync("ExtractAudio", videoContent, "test_video.mp4", options);

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