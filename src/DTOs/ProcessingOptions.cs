using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class ProcessingOptions
{
    /// <summary>
    /// Desired output format (e.g. "mp4", "mov").
    /// </summary>
    /// <example>mp4</example>
    public string? OutputFormat { get; set; }
    /// <summary>
    /// Encoding preset or quality profile for FFmpeg (e.g. "fast", "slow").
    /// </summary>
    /// <example>fast</example>
    public string? Quality { get; set; }
    /// <summary>
    /// Target video bitrate in kilobits per second.
    /// </summary>
    /// <example>1200</example>
    public int? BitrateKbps { get; set; }
    /// <summary>
    /// Target resolution as WIDTH:HEIGHT (e.g. "1280:720").
    /// </summary>
    /// <example>1280:720</example>
    public string? Resolution { get; set; }
    // Crop parameters: define an optional crop area.
    // If provided, FFmpeg will run `-vf "crop=CropWidth:CropHeight:CropX:CropY"`.
    /// <summary>
    /// Crop width in pixels. When set together with <see cref="CropHeight"/>, a crop filter will be applied.
    /// </summary>
    /// <example>640</example>
    public int? CropWidth { get; set; }
    /// <summary>
    /// Crop height in pixels.
    /// </summary>
    /// <example>360</example>
    public int? CropHeight { get; set; }
    /// <summary>
    /// X offset for crop (pixels). Defaults to 0 when omitted.
    /// </summary>
    /// <example>10</example>
    public int? CropX { get; set; }
    /// <summary>
    /// Y offset for crop (pixels). Defaults to 0 when omitted.
    /// </summary>
    /// <example>20</example>
    public int? CropY { get; set; }
    /// <summary>
    /// Start time in seconds for trimming operations.
    /// </summary>
    /// <example>5.0</example>
    public double? StartTime { get; set; }
    /// <summary>
    /// End time in seconds for trimming operations.
    /// </summary>
    /// <example>15.0</example>
    public double? EndTime { get; set; }
    /// <summary>
    /// Optional text watermark to render onto the video.
    /// </summary>
    /// <example>Sample watermark</example>
    public string? WatermarkText { get; set; }
    /// <summary>
    /// Optional file path to an image to be used as watermark.
    /// </summary>
    /// <example>/path/to/watermark.png</example>
    public string? WatermarkImagePath { get; set; }
}
