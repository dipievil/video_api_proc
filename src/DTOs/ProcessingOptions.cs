using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class ProcessingOptions
{
    public string? OutputFormat { get; set; }
    public string? Quality { get; set; }
    public int? BitrateKbps { get; set; }
    public string? Resolution { get; set; }
    // Crop parameters: define an optional crop area.
    // If provided, FFmpeg will run `-vf "crop=CropWidth:CropHeight:CropX:CropY"`.
    public int? CropWidth { get; set; }
    public int? CropHeight { get; set; }
    public int? CropX { get; set; }
    public int? CropY { get; set; }
    public double? StartTime { get; set; }
    public double? EndTime { get; set; }
    public string? WatermarkText { get; set; }
    public string? WatermarkImagePath { get; set; }
}
