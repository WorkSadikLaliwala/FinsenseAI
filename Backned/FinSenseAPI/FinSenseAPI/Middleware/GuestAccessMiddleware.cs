using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinSenseAPI.Middleware;

public class GuestAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GuestAccessMiddleware> _logger;

    public GuestAccessMiddleware(RequestDelegate next, ILogger<GuestAccessMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Standard middleware entry point. Adds a flag to HttpContext.Items indicating guest access
    // (not authenticated) and logs at debug level.
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
        var isGuest = !isAuthenticated;

        // Make guest status available to downstream middleware/controllers
        context.Items["IsGuest"] = isGuest;

        if (isGuest)
        {
            _logger.LogDebug("Guest access for {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        await _next(context);
    }
}
