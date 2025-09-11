namespace VideoProcessingApi.UnitTests.Services;

public class FFmpegErrorHandlerServiceTests
{
    private readonly FFmpegErrorHandlerService _errorHandler;

    public FFmpegErrorHandlerServiceTests()
    {
        _errorHandler = new FFmpegErrorHandlerService();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    public void MapError_WithNullOrEmptyInput_ShouldReturnNull(string? input, string? expected)
    {
        // Act
        var result = _errorHandler.MapError(input!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("failed to configure output pad")]
    [InlineData("Failed to configure output pad")]
    [InlineData("FAILED TO CONFIGURE OUTPUT PAD")]
    public void MapError_WithConfigureOutputPadError_ShouldReturnVideoSourcesMismatch(string errorMessage)
    {
        // Act
        var result = _errorHandler.MapError(errorMessage);

        // Assert
        result.Should().Be(FFmpegErrorMessages.VideoSourcesMismatch);
    }

    [Theory]
    [InlineData("input link parameters do not match")]
    [InlineData("Input Link Parameters Do Not Match")]
    [InlineData("INPUT LINK PARAMETERS DO NOT MATCH")]
    public void MapError_WithInputLinkMismatchError_ShouldReturnVideoSourcesMismatch(string errorMessage)
    {
        // Act
        var result = _errorHandler.MapError(errorMessage);

        // Assert
        result.Should().Be(FFmpegErrorMessages.VideoSourcesMismatch);
    }

    [Theory]
    [InlineData("error reinitializing filters")]
    [InlineData("Error Reinitializing Filters")]
    [InlineData("ERROR REINITIALIZING FILTERS")]
    public void MapError_WithReinitializingFiltersError_ShouldReturnVideoSourcesMismatch(string errorMessage)
    {
        // Act
        var result = _errorHandler.MapError(errorMessage);

        // Assert
        result.Should().Be(FFmpegErrorMessages.VideoSourcesMismatch);
    }

    [Theory]
    [InlineData("failed to inject frame into filter network")]
    [InlineData("Failed to inject frame into filter network")]
    [InlineData("FAILED TO INJECT FRAME INTO FILTER NETWORK")]
    public void MapError_WithInjectFrameError_ShouldReturnVideoSourcesMismatch(string errorMessage)
    {
        // Act
        var result = _errorHandler.MapError(errorMessage);

        // Assert
        result.Should().Be(FFmpegErrorMessages.VideoSourcesMismatch);
    }

    [Theory]
    [InlineData("Unknown error message")]
    [InlineData("File not found")]
    [InlineData("Permission denied")]
    [InlineData("Some random ffmpeg error")]
    public void MapError_WithUnknownError_ShouldReturnNull(string errorMessage)
    {
        // Act
        var result = _errorHandler.MapError(errorMessage);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MapError_WithComplexErrorMessage_ShouldDetectKnownErrors()
    {
        // Arrange
        var complexError = @"
            ffmpeg version 4.4.0 Copyright (c) 2000-2021
            [Parsed_concat_0 @ 0x55a8b8c0f5c0] failed to configure output pad
            [filter:v:0 @ 0x55a8b8c0f300] Error reinitializing filters!
            Failed to inject frame into filter network: Invalid argument
            Error while decoding stream #0:0: Invalid argument
        ";

        // Act
        var result = _errorHandler.MapError(complexError);

        // Assert
        result.Should().Be(FFmpegErrorMessages.VideoSourcesMismatch);
    }
}