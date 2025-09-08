using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using VideoProcessingApi.Configuration;
using VideoProcessingApi.DTOs;
using VideoProcessingApi.Interfaces;

namespace VideoProcessingApi.Services;

public class FFmpegService : IFFmpegService
{
    private readonly FFmpegSettings _settings;
    private readonly ILogger<FFmpegService> _logger;

    public FFmpegService(IOptions<FFmpegSettings> settings, ILogger<FFmpegService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
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

        if (!string.IsNullOrEmpty(options.Resolution))
        {
            arguments += $" -vf scale={options.Resolution}";
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
            Arguments = arguments,
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
}