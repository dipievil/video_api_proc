using System.Text.Json;
using FluentAssertions;
using VideoProcessingApi.IntegrationTests.Infrastructure;

namespace VideoProcessingApi.IntegrationTests.Tests;

[Collection("VideoProcessingApi")]
public class VideoTrimTests : IntegrationTestBase
{
    public VideoTrimTests(DockerComposeFixture dockerFixture) : base(dockerFixture)
    {
    }

    [Fact]
    public async Task CreateTrimJob_WithStartAndEndTime_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { startTime = 5.0, endTime = 15.0 };

        // Act
        var response = await ApiClient.CreateJobAsync("Trim", videoContent, "test_video.mp4", options);

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
    public async Task CreateTrimJob_FirstTenSeconds_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { startTime = 0.0, endTime = 10.0 };

        // Act
        var response = await ApiClient.CreateJobAsync("Trim", videoContent, "test_video.mp4", options);

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
    public async Task CreateTrimJob_MiddleSection_ShouldReturnSuccess()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { startTime = 10.5, endTime = 30.0 };

        // Act
        var response = await ApiClient.CreateJobAsync("Trim", videoContent, "test_video.mp4", options);

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
    public async Task CreateTrimJob_AndWaitForCompletion_ShouldSucceed()
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { startTime = 2.0, endTime = 8.0 };

        // Act - Create job
        var createResponse = await ApiClient.CreateJobAsync("Trim", videoContent, "test_video.mp4", options);
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
            throw new InvalidOperationException($"Trim job failed: {finalStatus.ErrorMessage}");
        }
    }

    [Theory]
    [InlineData(0.0, 5.0)]
    [InlineData(5.0, 10.0)]
    [InlineData(10.0, 20.0)]
    public async Task CreateTrimJob_WithDifferentTimeRanges_ShouldReturnSuccess(double startTime, double endTime)
    {
        // Arrange
        var videoContent = GetTestVideoContent();
        var options = new { startTime, endTime };

        // Act
        var response = await ApiClient.CreateJobAsync("Trim", videoContent, "test_video.mp4", options);

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