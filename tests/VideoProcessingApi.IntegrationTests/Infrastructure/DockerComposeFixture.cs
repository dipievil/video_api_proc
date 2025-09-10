using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VideoProcessingApi.IntegrationTests.Infrastructure;

public class DockerComposeFixture : IDisposable
{
    private readonly ILogger<DockerComposeFixture> _logger;
    private bool _isStarted = false;
    private bool _disposed = false;

    public string ApiBaseUrl { get; } = "http://localhost:5002";
    public string ApiKey { get; } = "test-api-key-12345";

    public DockerComposeFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DockerComposeFixture>();
    }

    public async Task StartAsync()
    {
        if (_isStarted) return;

        _logger.LogInformation("Starting Docker Compose for integration tests...");
        
        // Stop any existing containers
        await RunDockerComposeCommand("down", "--volumes");
        
        // Create necessary directories
        CreateTestDirectories();
        
        // Start containers
        await RunDockerComposeCommand("up", "-d", "--build");
        
        // Wait for services to be healthy
        await WaitForHealthyServices();
        
        _isStarted = true;
        _logger.LogInformation("Docker Compose services are ready for testing");
    }

    public async Task StopAsync()
    {
        if (!_isStarted) return;

        _logger.LogInformation("Stopping Docker Compose services...");
        await RunDockerComposeCommand("down", "--volumes");
        
        // Clean up test directories
        CleanupTestDirectories();
        
        _isStarted = false;
        _logger.LogInformation("Docker Compose services stopped");
    }

    private async Task RunDockerComposeCommand(params string[] args)
    {
        var dockerComposeCommand = await GetDockerComposeCommand();
        var arguments = $"-f docker-compose.test.yml {string.Join(" ", args)}";
        
        var processInfo = new ProcessStartInfo
        {
            FileName = dockerComposeCommand.Item1,
            Arguments = dockerComposeCommand.Item2 + " " + arguments,
            WorkingDirectory = GetProjectRootPath(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
            throw new InvalidOperationException($"Failed to start {dockerComposeCommand.Item1} process");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("Docker Compose command failed: {Command} {Arguments}", dockerComposeCommand.Item1, arguments);
            _logger.LogError("Output: {Output}", output);
            _logger.LogError("Error: {Error}", error);
            throw new InvalidOperationException($"Docker Compose command failed with exit code {process.ExitCode}");
        }

        _logger.LogDebug("Docker Compose output: {Output}", output);
    }

    private async Task<(string, string)> GetDockerComposeCommand()
    {
        // Try docker compose (v2) first
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "compose version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    return ("docker", "compose");
                }
            }
        }
        catch
        {
            // Ignore and try next option
        }

        // Try docker-compose (v1) as fallback
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    return ("docker-compose", "");
                }
            }
        }
        catch
        {
            // Ignore
        }

        throw new InvalidOperationException("Neither 'docker compose' nor 'docker-compose' is available. Please install Docker Compose.");
    }

    private async Task WaitForHealthyServices()
    {
        var maxRetries = 30; // 5 minutes with 10-second intervals
        var retryCount = 0;

        using var httpClient = new HttpClient();
        
        while (retryCount < maxRetries)
        {
            try
            {
                var response = await httpClient.GetAsync($"{ApiBaseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API health check passed");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Health check attempt {Attempt} failed: {Error}", retryCount + 1, ex.Message);
            }

            retryCount++;
            await Task.Delay(10000); // Wait 10 seconds between retries
        }

        throw new TimeoutException("Services did not become healthy within the expected time");
    }

    private void CreateTestDirectories()
    {
        var projectRoot = GetProjectRootPath();
        var testDirs = new[]
        {
            Path.Combine(projectRoot, "tests", "uploads"),
            Path.Combine(projectRoot, "tests", "processed"),
            Path.Combine(projectRoot, "tests", "db"),
            Path.Combine(projectRoot, "tests", "logs")
        };

        foreach (var dir in testDirs)
        {
            Directory.CreateDirectory(dir);
            _logger.LogDebug("Created test directory: {Directory}", dir);
        }
    }

    private void CleanupTestDirectories()
    {
        var projectRoot = GetProjectRootPath();
        var testDirs = new[]
        {
            Path.Combine(projectRoot, "tests", "uploads"),
            Path.Combine(projectRoot, "tests", "processed"),
            Path.Combine(projectRoot, "tests", "db"),
            Path.Combine(projectRoot, "tests", "logs")
        };

        foreach (var dir in testDirs)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    _logger.LogDebug("Cleaned up test directory: {Directory}", dir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to cleanup directory {Directory}: {Error}", dir, ex.Message);
                }
            }
        }
    }

    private static string GetProjectRootPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = currentDirectory;

        // Navigate up until we find the project root (contains docker-compose.test.yml)
        while (!File.Exists(Path.Combine(projectRoot, "docker-compose.test.yml")))
        {
            var parent = Directory.GetParent(projectRoot);
            if (parent == null)
                throw new InvalidOperationException("Could not find project root directory");
            
            projectRoot = parent.FullName;
        }

        return projectRoot;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}