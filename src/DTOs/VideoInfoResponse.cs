namespace VideoProcessingApi.DTOs;

/// <summary>
/// Response containing video file metadata information.
/// </summary>
public class VideoInfoResponse
{
    /// <summary>
    /// Video width in pixels.
    /// </summary>
    /// <example>1920</example>
    public int Width { get; set; }

    /// <summary>
    /// Video height in pixels.
    /// </summary>
    /// <example>1080</example>
    public int Height { get; set; }

    /// <summary>
    /// Aspect ratio as a formatted string.
    /// </summary>
    /// <example>16:9</example>
    public string AspectRatio { get; set; } = string.Empty;

    /// <summary>
    /// Video duration in seconds.
    /// </summary>
    /// <example>120.5</example>
    public double Duration { get; set; }

    /// <summary>
    /// Video bitrate in kilobits per second.
    /// </summary>
    /// <example>2500</example>
    public int Bitrate { get; set; }

    /// <summary>
    /// Video frame rate (frames per second).
    /// </summary>
    /// <example>30.0</example>
    public double FrameRate { get; set; }

    /// <summary>
    /// Video codec used for encoding.
    /// </summary>
    /// <example>h264</example>
    public string Codec { get; set; } = string.Empty;

    /// <summary>
    /// Audio codec used for encoding.
    /// </summary>
    /// <example>aac</example>
    public string AudioCodec { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    /// <example>15728640</example>
    public long FileSize { get; set; }

    /// <summary>
    /// Original filename.
    /// </summary>
    /// <example>sample.mp4</example>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Video format/container.
    /// </summary>
    /// <example>mp4</example>
    public string Format { get; set; } = string.Empty;
}