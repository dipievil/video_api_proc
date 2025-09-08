using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using VideoProcessingApi.Configuration;

namespace VideoProcessingApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecuritySettings _settings;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requestHistory = new();

    public RateLimitingMiddleware(RequestDelegate next, IOptions<SecuritySettings> settings, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var now = DateTime.UtcNow;

        if (!_requestHistory.TryGetValue(clientId, out var requests))
        {
            requests = new List<DateTime>();
            _requestHistory[clientId] = requests;
        }

        // Remove old requests (older than 1 minute)
        requests.RemoveAll(r => r < now.AddMinutes(-1));

        if (requests.Count >= _settings.RateLimit.MaxRequestsPerMinute)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        requests.Add(now);
        await _next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        return $"{apiKey ?? "anonymous"}_{ipAddress}";
    }
}
