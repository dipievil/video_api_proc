using System.Text.Json.Serialization;
using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class ProcessingOptions
{
    /// <summary>
    /// Desired output format (e.g. "mp4", "mov").
    /// </summary>
    [JsonPropertyName("OutputFormat")]
    public string? OutputFormat { get; set; }
    /// <summary>
    /// Encoding preset or quality profile for FFmpeg (e.g. "fast", "slow").
    /// </summary>
    public string? Quality { get; set; }
    /// <summary>
    /// Target video bitrate in kilobits per second (e.g. 1200).
    /// </summary>
    public int? BitrateKbps { get; set; }
    /// <summary>
    /// Target resolution as WIDTH:HEIGHT (e.g. "1280:720").
    /// </summary>
    public string? Resolution { get; set; }
    /// <summary>
    /// Crop parameters: define an optional crop area.
    /// If provided, FFmpeg will run `-vf "crop=CropWidth:CropHeight:CropX:CropY"`.
    /// When set together with <see cref="CropHeight"/>, a crop filter will be applied (e.g. 640).
    /// </summary>
    public int? CropWidth { get; set; }
    /// <summary>
    /// Crop parameters: define an optional crop area.
    /// If provided, FFmpeg will run `-vf "crop=CropWidth:CropHeight:CropX:CropY"`.
    /// When set together with <see cref="CropWidth"/>, a crop filter will be applied (e.g. 280).
    /// </summary>
    public int? CropHeight { get; set; }
    /// <summary>
    /// X offset for crop (pixels). Defaults to 0 when omitted (e.g. 10).
    /// </summary>
    public int? CropX { get; set; }
    /// <summary>
    /// Y offset for crop (pixels). Defaults to 0 when omitted (e.g. 20).
    /// </summary>
    public int? CropY { get; set; }
    /// <summary>
    /// Start time in seconds for trimming operations (e.g. 5.0).
    /// </summary>
    public double? StartTime { get; set; }
    /// <summary>
    /// End time in seconds for trimming operations (e.g. 15.0).
    /// </summary>
    public double? EndTime { get; set; }
    /// <summary>
    /// Optional text watermark to render onto the video (e.g. Sample watermark).
    /// </summary>
    public string? WatermarkText { get; set; }
    /// <summary>
    /// Optional file path to an image to be used as watermark (e.g. /path/to/watermark.png).
    /// </summary>
    public string? WatermarkImagePath { get; set; }
}
