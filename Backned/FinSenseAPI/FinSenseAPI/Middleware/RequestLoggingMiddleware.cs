using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinSenseAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Example: log request start
            _logger.LogInformation("Handling request {Method} {Path}", context.Request.Method, context.Request.Path);

            // Call the next middleware
            await _next(context);

            // Example: log request end
            _logger.LogInformation("Finished handling request. Response status: {StatusCode}", context.Response.StatusCode);
        }
    }
}