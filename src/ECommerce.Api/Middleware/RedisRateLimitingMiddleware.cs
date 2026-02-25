using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ECommerce.Api.Settings;

namespace ECommerce.Api.Middleware;

public class RedisRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RedisRateLimitingMiddleware> _logger;
    private readonly RateLimitSettings _settings;

    public RedisRateLimitingMiddleware(
        RequestDelegate next, 
        ILogger<RedisRateLimitingMiddleware> logger,
        IOptions<RateLimitSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
    {
        // Skip rate limiting for certain paths if needed (e.g., health checks)
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
        // Fixed window key depending on the WindowMinutes
        var windowKey = DateTime.UtcNow.Ticks / (TimeSpan.TicksPerMinute * _settings.WindowMinutes);
        var cacheKey = $"rate_limit:{ipAddress}:{windowKey}";

        var db = redis.GetDatabase();

        // Atomically increment the request count for this IP in the current window
        var requestCount = await db.StringIncrementAsync(cacheKey);

        // If it's the first request in this window, set the expiration so we don't leak memory
        if (requestCount == 1)
        {
            await db.KeyExpireAsync(cacheKey, TimeSpan.FromMinutes(_settings.WindowMinutes + 1));
        }

        if (requestCount > _settings.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {IPAddress}. Count: {Count}", ipAddress, requestCount);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            
            // Add retry-after header
            var secondsLeft = (int)(TimeSpan.FromMinutes(_settings.WindowMinutes).TotalSeconds - (DateTime.UtcNow.Second % TimeSpan.FromMinutes(_settings.WindowMinutes).TotalSeconds));
            context.Response.Headers.Append("Retry-After", secondsLeft.ToString());

            await context.Response.WriteAsync($@"{{
                ""error"": ""Too many requests"",
                ""message"": ""You have exceeded the limit of {_settings.MaxRequests} requests per {_settings.WindowMinutes} minute(s)."",
                ""retryAfterSeconds"": {secondsLeft}
            }}");
            
            return;
        }

        // Add rate limit context headers for the client
        context.Response.Headers.Append("X-RateLimit-Limit", _settings.MaxRequests.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", (_settings.MaxRequests - requestCount).ToString());

        await _next(context);
    }
}
