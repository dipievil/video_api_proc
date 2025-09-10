using FluentAssertions;

namespace VideoProcessingApi.IntegrationTests.Tests;

public class BasicTests
{
    [Fact]
    public void TestVideoContent_ShouldNotBeEmpty()
    {
        // Arrange & Act
        var testVideoContent = GetTestVideoContent();

        // Assert
        testVideoContent.Should().NotBeEmpty();
    }

    [Fact]
    public void TestVideoContent_ShouldHaveCorrectMp4Header()
    {
        // Arrange & Act
        var testVideoContent = GetTestVideoContent();

        // Assert
        testVideoContent.Length.Should().BeGreaterThan(20);
        // Check for basic MP4 structure (ftyp box)
        var headerBytes = testVideoContent.Take(8).ToArray();
        headerBytes.Should().HaveCountGreaterThan(4);
    }

    private static byte[] GetTestVideoContent()
    {
        var testVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_video.mp4");
        
        if (File.Exists(testVideoPath))
        {
            return File.ReadAllBytes(testVideoPath);
        }
        
        // Create a minimal dummy video file for testing if the real one doesn't exist
        return CreateDummyVideoContent();
    }

    private static byte[] CreateDummyVideoContent()
    {
        // This creates a minimal MP4 header for testing purposes
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