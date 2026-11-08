namespace SchoolAdmission.Api.DTOs;

/// <summary>
/// Uniform body for every 4xx/5xx response. Success (2xx) bodies are never wrapped.
/// <c>message</c> is human-readable and safe to show in the UI; <c>errors</c> maps
/// field name -> messages (empty object when there are no field errors).
/// </summary>
public sealed record ErrorResponse(
    int StatusCode,
    string Message,
    IReadOnlyDictionary<string, string[]> Errors,
    DateTimeOffset Timestamp)
{
    public static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    public static ErrorResponse Create(int statusCode, string message,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(statusCode, message, errors ?? NoErrors, DateTimeOffset.UtcNow);
}
