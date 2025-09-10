using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class VideoMergeTests : IntegrationTestBase
{
    public VideoMergeTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task CreateMergeJob_WithMultipleVideos_ShouldReturnSuccess()
    {
        // Arrange
        var video1Content = GetTestVideoContent();
        var video2Content = GetTestVideoContent();
        var videoContents = new List<byte[]> { video1Content, video2Content };
        var fileNames = new List<string> { "video1.mp4", "video2.mp4" };

        // Act
        var response = await ApiClient.CreateMergeJobAsync(videoContents, fileNames);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jobResponse.Should().NotBeNull();
        jobResponse!.JobId.Should().NotBeEmpty();
        jobResponse.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateMergeJob_AndWaitForCompletion_ShouldSucceed()
    {
        // Arrange
        var video1Content = GetTestVideoContent();
        var video2Content = GetTestVideoContent();
        var videoContents = new List<byte[]> { video1Content, video2Content };
        var fileNames = new List<string> { "video1.mp4", "video2.mp4" };

        // Act - Create job
        var createResponse = await ApiClient.CreateMergeJobAsync(videoContents, fileNames);
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
            // Log the error for debugging
            finalStatus.ErrorMessage.Should().NotBeNullOrEmpty();
            throw new InvalidOperationException($"Job failed: {finalStatus.ErrorMessage}");
        }
    }

    [Fact]
    public async Task GetJobStatus_ForExistingJob_ShouldReturnStatus()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var createResponse = await ApiClient.CreateJobAsync("Merge", videoContent);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var jobResponse = JsonSerializer.Deserialize<CreateJobResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act
        var statusResponse = await ApiClient.GetJobStatusAsync(jobResponse!.JobId);

        // Assert
        statusResponse.IsSuccessStatusCode.Should().BeTrue();
        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<JobStatusResponse>(statusContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        status.Should().NotBeNull();
        status!.JobId.Should().Be(jobResponse.JobId);
        status.Status.Should().BeOneOf("Pending", "Processing", "Completed", "Failed");
    }

    [Fact]
    public async Task GetJobStatus_ForNonExistentJob_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetJobStatusAsync(nonExistentJobId);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}