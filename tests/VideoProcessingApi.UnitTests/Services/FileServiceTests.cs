using Microsoft.AspNetCore.Http;

namespace VideoProcessingApi.UnitTests.Services;

public class FileServiceTests
{
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _mockStorageService = new Mock<IStorageService>();
        _mockLogger = new Mock<ILogger<FileService>>();
        _fileService = new FileService(_mockStorageService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldCallStorageService()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var directory = "/uploads";
        var expectedPath = "/uploads/test.mp4";
        
        _mockStorageService
            .Setup(x => x.SaveFileAsync(mockFile.Object, directory))
            .ReturnsAsync(expectedPath);

        // Act
        var result = await _fileService.SaveFileAsync(mockFile.Object, directory);

        // Assert
        result.Should().Be(expectedPath);
        _mockStorageService.Verify(x => x.SaveFileAsync(mockFile.Object, directory), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldCallStorageService()
    {
        // Arrange
        var filePath = "/uploads/test.mp4";
        
        _mockStorageService
            .Setup(x => x.DeleteFileAsync(filePath))
            .ReturnsAsync(true);

        // Act
        var result = await _fileService.DeleteFileAsync(filePath);

        // Assert
        result.Should().BeTrue();
        _mockStorageService.Verify(x => x.DeleteFileAsync(filePath), Times.Once);
    }

    [Theory]
    [InlineData("test.mp4", new[] { ".mp4", ".avi" }, true)]
    [InlineData("test.MP4", new[] { ".mp4", ".avi" }, true)]
    [InlineData("test.avi", new[] { ".mp4", ".avi" }, true)]
    [InlineData("test.mov", new[] { ".mp4", ".avi" }, false)]
    [InlineData("test.txt", new[] { ".mp4", ".avi" }, false)]
    public void ValidateFileType_ShouldReturnExpectedResult(string fileName, string[] allowedTypes, bool expected)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns(fileName);

        // Act
        var result = _fileService.ValidateFileType(mockFile.Object, allowedTypes);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1024, 2048, true)]
    [InlineData(2048, 2048, true)]
    [InlineData(2049, 2048, false)]
    [InlineData(0, 1, true)]
    public void ValidateFileSize_ShouldReturnExpectedResult(long fileSize, long maxSize, bool expected)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.Length).Returns(fileSize);

        // Act
        var result = _fileService.ValidateFileSize(mockFile.Object, maxSize);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetFileStreamAsync_ShouldCallStorageService()
    {
        // Arrange
        var filePath = "/uploads/test.mp4";
        var expectedStream = new MemoryStream();
        
        _mockStorageService
            .Setup(x => x.GetFileStreamAsync(filePath))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _fileService.GetFileStreamAsync(filePath);

        // Assert
        result.Should().BeSameAs(expectedStream);
        _mockStorageService.Verify(x => x.GetFileStreamAsync(filePath), Times.Once);
    }
}