using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace FinSenseAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case ValidationException ve:
                statusCode = StatusCodes.Status400BadRequest;
                message = ve.Message;
                break;
            case UnauthorizedAccessException ua:
                statusCode = StatusCodes.Status401Unauthorized;
                message = ua.Message;
                break;
            case KeyNotFoundException knf:
                statusCode = StatusCodes.Status404NotFound;
                message = knf.Message;
                break;
            case HttpRequestException hre:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = hre.Message;
                break;
            case OperationCanceledException oce:
                statusCode = 499; // Client Closed Request (non-standard)
                message = "Request cancelled";
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred";
                break;
        }

        // Log with Serilog
        try
        {
            Log.Error(exception, "Unhandled exception caught by GlobalExceptionMiddleware");
        }
        catch
        {
            // swallow logging errors
        }

        var payload = new
        {
            success = false,
            message,
            data = (object?)null,
            error = exception.Message,
            statusCode
        };

        var result = JsonSerializer.Serialize(payload);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(result);
    }
}
