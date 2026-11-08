using Microsoft.AspNetCore.WebUtilities;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services.Exceptions;

namespace SchoolAdmission.Api.Middleware;

/// <summary>
/// Single place that produces error bodies:
///  1. AppException (404/409/400 from services)  -> ErrorResponse with its status + message
///  2. BadHttpRequestException (framework 4xx)   -> ErrorResponse with its status
///  3. anything else                             -> 500 ErrorResponse (details only in logs)
///  4. any 4xx/5xx that finished with no body    -> ErrorResponse (e.g. unknown route 404, 405, 415)
/// 2xx/3xx responses are never touched.
/// </summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message);
            return;
        }
        catch (BadHttpRequestException ex)
        {
            await WriteAsync(context, ex.StatusCode, "The request is malformed.");
            return;
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client went away; nothing useful to send.
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                "Something went wrong on our side. Please try again.");
            return;
        }

        // Error status set by the framework without a body (routing 404, 405, 415, ...).
        if (context.Response.StatusCode >= 400 && !context.Response.HasStarted &&
            context.Response.ContentLength is null or 0)
        {
            await WriteAsync(context, context.Response.StatusCode, DefaultMessage(context.Response.StatusCode));
        }
    }

    private async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning("Response already started; cannot write error body ({Status})", statusCode);
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(ErrorResponse.Create(statusCode, message));
    }

    private static string DefaultMessage(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status405MethodNotAllowed => "This HTTP method is not allowed for this URL.",
        StatusCodes.Status415UnsupportedMediaType => "Content-Type must be application/json.",
        _ => ReasonPhrases.GetReasonPhrase(statusCode) is { Length: > 0 } p ? p + "." : "Request failed.",
    };
}
