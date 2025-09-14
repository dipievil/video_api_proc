using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;

namespace VideoProcessingApi.IntegrationTests.Infrastructure;

public class ApiTestClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ApiTestClient(string baseUrl, string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }

    public async Task<HttpResponseMessage> CreateJobAsync(string processingType, byte[] videoContent, string fileName = "test_video.mp4", object? options = null)
    {
        using var form = new MultipartFormDataContent();
        
        // Add processing type
        form.Add(new StringContent(processingType), "processingType");
        
        // Add video file
        var fileContent = new ByteArrayContent(videoContent);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        form.Add(fileContent, "files", fileName);
        
        // Add options if provided
        if (options != null)
        {
            var optionsJson = JsonSerializer.Serialize(options);
            form.Add(new StringContent(optionsJson), "options");
        }

        return await _httpClient.PostAsync("/api/jobs", form);
    }

    public async Task<HttpResponseMessage> CreateMergeJobAsync(List<byte[]> videoContents, List<string> fileNames)
    {
        using var form = new MultipartFormDataContent();
        
        // Add processing type
        form.Add(new StringContent("Merge"), "processingType");
        
        // Add video files
        for (int i = 0; i < videoContents.Count; i++)
        {
            var fileContent = new ByteArrayContent(videoContents[i]);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
            form.Add(fileContent, "files", fileNames[i]);
        }

        return await _httpClient.PostAsync("/api/jobs", form);
    }

    public async Task<HttpResponseMessage> GetJobStatusAsync(Guid jobId)
    {
        return await _httpClient.GetAsync($"/api/jobs/{jobId}/status");
    }

    public async Task<HttpResponseMessage> DownloadJobResultAsync(Guid jobId)
    {
        return await _httpClient.GetAsync($"/api/jobs/{jobId}/download");
    }

    public async Task<HttpResponseMessage> CancelJobAsync(Guid jobId)
    {
        return await _httpClient.DeleteAsync($"/api/jobs/{jobId}");
    }

    public async Task<HttpResponseMessage> HealthCheckAsync()
    {
        return await _httpClient.GetAsync("/health");
    }

    public async Task<HttpResponseMessage> GetVideoInfoAsync(byte[] videoContent, string fileName = "test_video.mp4")
    {
        using var form = new MultipartFormDataContent();
        
        // Add video file
        var fileContent = new ByteArrayContent(videoContent);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
        form.Add(fileContent, "file", fileName);

        return await _httpClient.PostAsync("/api/videos/info", form);
    }

    public async Task<JobStatusResponse> WaitForJobCompletionAsync(Guid jobId, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromMinutes(5);
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < maxWait)
        {
            var response = await GetJobStatusAsync(jobId);
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var content = await response.Content.ReadAsStringAsync();
            var status = JsonSerializer.Deserialize<JobStatusResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (status?.Status == "Completed" || status?.Status == "Failed" || status?.Status == "Canceled")
            {
                return status;
            }

            await Task.Delay(2000); // Wait 2 seconds between checks
        }

        throw new TimeoutException($"Job {jobId} did not complete within {maxWait}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

public class JobStatusResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int Progress { get; set; }
    public string? OutputPath { get; set; }
}

public class CreateJobResponse
{
    public Guid JobId { get; set; }
    public string Message { get; set; } = string.Empty;
}