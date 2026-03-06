using System.Net;
using System.Text.Json;

namespace NgrApi.Middleware;

/// <summary>
/// Global exception handling middleware — returns RFC 7807 Problem Details (application/problem+json).
/// Logs structured errors without PHI/PII in the log message.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionType} at {Path}",
                ex.GetType().Name, context.Request.Path.Value);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ArgumentException e      => (HttpStatusCode.BadRequest,         "Invalid Request",      e.Message),
            InvalidOperationException e => (HttpStatusCode.Conflict,         "Operation Not Allowed", e.Message),
            KeyNotFoundException _   => (HttpStatusCode.NotFound,           "Resource Not Found",   "The requested resource was not found."),
            UnauthorizedAccessException _ => (HttpStatusCode.Forbidden,     "Access Denied",        "You do not have permission to perform this action."),
            _                        => (HttpStatusCode.InternalServerError, "Server Error",         "An unexpected error occurred. Please try again or contact support.")
        };

        var problem = new
        {
            type    = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status  = (int)statusCode,
            detail,
            instance = context.Request.Path.Value,
            traceId  = context.TraceIdentifier,
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
