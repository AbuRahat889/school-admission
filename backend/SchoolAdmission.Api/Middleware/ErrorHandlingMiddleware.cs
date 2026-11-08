using Microsoft.AspNetCore.WebUtilities;
using Npgsql;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services.Exceptions;

namespace SchoolAdmission.Api.Middleware;

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
        catch (Exception ex) when (FindPostgresError(ex) is { } pg && ClientErrorFor(pg) is { } mapped)
        {
            // Defense in depth: input that slipped past validation but the DB rejected is still the client's fault.
            logger.LogWarning(ex, "Database rejected input ({SqlState}) for {Method} {Path}",
                pg.SqlState, context.Request.Method, context.Request.Path);
            await WriteAsync(context, mapped.Status, mapped.Message);
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

    private static PostgresException? FindPostgresError(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
            if (ex is PostgresException pg) return pg;
        return null;
    }

    // SQLSTATE class 22 = data exception (bad characters, out of range, ...); 23514 = check constraint;
    // 23505 = unique violation (e.g. a second active hold). Anything else stays a 500.
    private static (int Status, string Message)? ClientErrorFor(PostgresException pg) => pg.SqlState switch
    {
        PostgresErrorCodes.UniqueViolation => (StatusCodes.Status409Conflict, "This conflicts with an existing record."),
        PostgresErrorCodes.CheckViolation => (StatusCodes.Status400BadRequest, "One or more values are not allowed."),
        var s when s.StartsWith("22", StringComparison.Ordinal) =>
            (StatusCodes.Status400BadRequest, "One or more values are invalid or contain unsupported characters."),
        _ => null,
    };

    private static string DefaultMessage(int statusCode) => statusCode switch
    {
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status405MethodNotAllowed => "This HTTP method is not allowed for this URL.",
        StatusCodes.Status415UnsupportedMediaType => "Content-Type must be application/json.",
        _ => ReasonPhrases.GetReasonPhrase(statusCode) is { Length: > 0 } p ? p + "." : "Request failed.",
    };
}
