using Microsoft.Extensions.Options;
using VideoProcessingApi.Configuration;

namespace VideoProcessingApi.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecuritySettings _settings;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<SecuritySettings> settings, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API key validation for specific paths
        var skipPaths = new[]
        {
            "/health",
            "/swagger",
            "/swagger/",
            "/swagger/index.html",
            "/swagger/v1/swagger.json",
            "/swagger/swagger-ui-bundle.js",
            "/swagger/swagger-ui-standalone-preset.js",
            "/swagger/swagger-ui.css",
            "/favicon.ico"
        };

        if (skipPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        if (!_settings.ApiKeys.Any(key => key == extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
