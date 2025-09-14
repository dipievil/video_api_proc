using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;
using VideoProcessingApi.Services;

namespace VideoProcessingApi.Services;

public class FFmpegService : IFFmpegService
{
    private readonly FFmpegSettings _settings;
    private readonly ILogger<FFmpegService> _logger;
    private readonly IFFmpegErrorHandlerService _errorHandler;

    public FFmpegService(IOptions<FFmpegSettings> settings, ILogger<FFmpegService> logger, IFFmpegErrorHandlerService errorHandler)
    {
        _settings = settings.Value;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    public async Task<string> MergeVideosAsync(List<string> inputPaths, string outputPath, ProcessingOptions? options = null)
    {
        var quality = options?.Quality ?? _settings.DefaultQuality;
        var inputList = string.Join(" ", inputPaths.Select((path, index) => $"-i \"{path}\""));
        var filterComplex = string.Join("", inputPaths.Select((_, index) => $"[{index}:v][{index}:a]"));
        filterComplex += $"concat=n={inputPaths.Count}:v=1:a=1[outv][outa]";

        var arguments = $"{inputList} -filter_complex \"{filterComplex}\" -map \"[outv]\" -map \"[outa]\" -c:v libx264 -preset {quality} -c:a aac \"{outputPath}\"";

        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<string> ConvertVideoAsync(string inputPath, string outputPath, ProcessingOptions options)
    {
        var arguments = $"-i \"{inputPath}\"";

        // Build video filters (scale, crop, etc.)
        var vfParts = new List<string>();

        if (!string.IsNullOrEmpty(options.Resolution))
        {
            vfParts.Add($"scale={options.Resolution}");
        }

        // If crop params are present, add crop filter
        if (options.CropWidth.HasValue && options.CropHeight.HasValue)
        {
            // default x/y to 0 when not provided
            var x = options.CropX ?? 0;
            var y = options.CropY ?? 0;
            vfParts.Add($"crop={options.CropWidth}:{options.CropHeight}:{x}:{y}");
        }

        if (vfParts.Any())
        {
            var vf = string.Join(",", vfParts);
            arguments += $" -vf \"{vf}\"";
        }

        if (options.BitrateKbps.HasValue)
        {
            arguments += $" -b:v {options.BitrateKbps}k";
        }

        arguments += $" -c:v libx264 -preset {options.Quality ?? _settings.DefaultQuality} \"{outputPath}\"";

        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<string> CompressVideoAsync(string inputPath, string outputPath, ProcessingOptions options)
    {
        var arguments = $"-i \"{inputPath}\"";

        // Build video filters (crop supported for compression flow too)
        var vfParts = new List<string>();

        if (options.CropWidth.HasValue && options.CropHeight.HasValue)
        {
            var x = options.CropX ?? 0;
            var y = options.CropY ?? 0;
            vfParts.Add($"crop={options.CropWidth}:{options.CropHeight}:{x}:{y}");
        }

        if (!string.IsNullOrEmpty(options.Resolution))
        {
            vfParts.Add($"scale={options.Resolution}");
        }

        if (vfParts.Any())
        {
            var vf = string.Join(",", vfParts);
            arguments += $" -vf \"{vf}\"";
        }

        if (options.BitrateKbps.HasValue)
        {
            arguments += $" -b:v {options.BitrateKbps}k";
        }

        arguments += $" -c:v libx264 -preset {options.Quality ?? _settings.DefaultQuality} \"{outputPath}\"";
        
        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<string> TrimVideoAsync(string inputPath, string outputPath, double startTime, double endTime)
    {
        var duration = endTime - startTime;
        var arguments = $"-i \"{inputPath}\" -ss {startTime} -t {duration} -c copy \"{outputPath}\"";
        
        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<string> ExtractAudioAsync(string inputPath, string outputPath, ProcessingOptions? options = null)
    {
        var arguments = $"-i \"{inputPath}\" -vn -acodec copy \"{outputPath}\"";
        
        return await ExecuteFFmpegAsync(arguments);
    }

    public async Task<bool> ValidateVideoFileAsync(string filePath)
    {
        try
        {
            var arguments = $"-v error -select_streams v:0 -show_entries stream=codec_name -of csv=p=0 \"{filePath}\"";
            var result = await ExecuteFFmpegAsync(arguments);
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> ExecuteFFmpegAsync(string arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _settings.BinaryPath,
            // reduce verbosity for easier parsing
            Arguments = $"-hide_banner {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null) outputBuilder.AppendLine(args.Data);
        };
        
        process.ErrorDataReceived += (sender, args) => {
            if (args.Data != null) errorBuilder.AppendLine(args.Data);
        };

        _logger.LogInformation("Executing FFmpeg with arguments: {Arguments}", arguments);
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_settings.TimeoutMinutes));
        await process.WaitForExitAsync(cancellationTokenSource.Token);

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (process.ExitCode != 0)
        {
            _logger.LogError("FFmpeg execution failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
            // try map to friendly message
            var friendly = _errorHandler.MapError(error);
            if (!string.IsNullOrEmpty(friendly))
            {
                throw new InvalidOperationException(friendly);
            }

            throw new InvalidOperationException($"FFmpeg execution failed: {error}");
        }

        _logger.LogInformation("FFmpeg execution completed successfully");
        return output;
    }

    public async Task<bool> IsFFmpegAvailableAsync()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _settings.BinaryPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking FFmpeg availability");
            return false;
        }
    }

    public async Task<VideoInfoResponse> GetVideoInfoAsync(string filePath, string filename)
    {
        try
        {
            var probePath = _settings.BinaryPath.Replace("ffmpeg", "ffprobe");
            var arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = probePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            _logger.LogInformation("Executing FFprobe with arguments: {Arguments}", arguments);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_settings.TimeoutMinutes));
            await process.WaitForExitAsync(cancellationTokenSource.Token);

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (process.ExitCode != 0)
            {
                _logger.LogError("FFprobe execution failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"FFprobe execution failed: {error}");
            }

            return ParseVideoInfo(output, filePath, filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video info for file: {FilePath}", filePath);
            throw;
        }
    }

    private VideoInfoResponse ParseVideoInfo(string jsonOutput, string filePath, string filename)
    {
        using var document = JsonDocument.Parse(jsonOutput);
        var root = document.RootElement;

        var format = root.GetProperty("format");
        var streams = root.GetProperty("streams");

        // Find video stream
        JsonElement videoStream = default;
        JsonElement audioStream = default;

        foreach (var stream in streams.EnumerateArray())
        {
            var codecType = stream.GetProperty("codec_type").GetString();
            if (codecType == "video" && videoStream.ValueKind == JsonValueKind.Undefined)
            {
                videoStream = stream;
            }
            else if (codecType == "audio" && audioStream.ValueKind == JsonValueKind.Undefined)
            {
                audioStream = stream;
            }
        }

        if (videoStream.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException("No video stream found in the file");
        }

        var width = videoStream.GetProperty("width").GetInt32();
        var height = videoStream.GetProperty("height").GetInt32();
        var duration = double.Parse(format.GetProperty("duration").GetString() ?? "0");
        var bitrate = format.TryGetProperty("bit_rate", out var bitrateProperty) 
            ? int.Parse(bitrateProperty.GetString() ?? "0") / 1000 // Convert to kbps
            : 0;

        var frameRateStr = videoStream.TryGetProperty("r_frame_rate", out var frameRateProp) 
            ? frameRateProp.GetString() 
            : "0/1";
        var frameRate = ParseFrameRate(frameRateStr ?? "0/1");

        var codec = videoStream.GetProperty("codec_name").GetString() ?? "";
        var audioCodec = audioStream.ValueKind != JsonValueKind.Undefined 
            ? audioStream.GetProperty("codec_name").GetString() ?? ""
            : "";

        var fileInfo = new FileInfo(filePath);
        var fileSize = fileInfo.Exists ? fileInfo.Length : 0;

        var formatName = format.GetProperty("format_name").GetString() ?? "";

        return new VideoInfoResponse
        {
            Width = width,
            Height = height,
            AspectRatio = CalculateAspectRatio(width, height),
            Duration = duration,
            Bitrate = bitrate,
            FrameRate = frameRate,
            Codec = codec,
            AudioCodec = audioCodec,
            FileSize = fileSize,
            Filename = filename,
            Format = formatName.Split(',')[0] // Take first format name
        };
    }

    private static double ParseFrameRate(string frameRateStr)
    {
        try
        {
            var parts = frameRateStr.Split('/');
            if (parts.Length == 2 && 
                double.TryParse(parts[0], out var numerator) && 
                double.TryParse(parts[1], out var denominator) && 
                denominator != 0)
            {
                return numerator / denominator;
            }
        }
        catch
        {
            // Ignore parse errors
        }
        return 0;
    }

    private static string CalculateAspectRatio(int width, int height)
    {
        if (height == 0) return "0:0";

        var gcd = GreatestCommonDivisor(width, height);
        return $"{width / gcd}:{height / gcd}";
    }

    private static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}