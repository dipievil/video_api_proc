using VideoProcessingApi.Enums;

namespace VideoProcessingApi.DTOs;

public class ProcessingOptions
{
    public string? OutputFormat { get; set; }
    public string? Quality { get; set; }
    public int? BitrateKbps { get; set; }
    public string? Resolution { get; set; }
    public double? StartTime { get; set; }
    public double? EndTime { get; set; }
    public string? WatermarkText { get; set; }
    public string? WatermarkImagePath { get; set; }
}
