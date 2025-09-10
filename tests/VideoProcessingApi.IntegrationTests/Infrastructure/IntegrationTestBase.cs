using FluentAssertions;

namespace VideoProcessingApi.IntegrationTests.Infrastructure;

[CollectionDefinition("VideoProcessingApi")]
public class VideoProcessingApiCollection : ICollectionFixture<DockerComposeFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly DockerComposeFixture DockerFixture;
    protected readonly ApiTestClient ApiClient;

    protected IntegrationTestBase(DockerComposeFixture dockerFixture)
    {
        DockerFixture = dockerFixture;
        ApiClient = new ApiTestClient(DockerFixture.ApiBaseUrl, DockerFixture.ApiKey);
    }

    public virtual async Task InitializeAsync()
    {
        await DockerFixture.StartAsync();
        
        // Ensure API is healthy
        var healthResponse = await ApiClient.HealthCheckAsync();
        healthResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    public virtual Task DisposeAsync()
    {
        ApiClient?.Dispose();
        return Task.CompletedTask;
    }

    protected static byte[] GetTestVideoContent()
    {
        var testVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_video.mp4");
        
        if (!File.Exists(testVideoPath))
        {
            // Create a minimal dummy video file for testing if the real one doesn't exist
            return CreateDummyVideoContent();
        }
        
        return File.ReadAllBytes(testVideoPath);
    }

    private static byte[] CreateDummyVideoContent()
    {
        // This creates a minimal MP4 header for testing purposes
        // In a real scenario, you'd want to use the actual test video
        var dummyContent = new byte[]
        {
            0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, // ftyp box
            0x69, 0x73, 0x6F, 0x6D, 0x00, 0x00, 0x02, 0x00,
            0x69, 0x73, 0x6F, 0x6D, 0x69, 0x73, 0x6F, 0x32,
            0x61, 0x76, 0x63, 0x31, 0x6D, 0x70, 0x34, 0x31
        };
        
        return dummyContent;
    }
}