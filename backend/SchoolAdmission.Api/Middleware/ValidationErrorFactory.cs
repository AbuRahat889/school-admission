using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolAdmission.Api.DTOs;

namespace SchoolAdmission.Api.Middleware;


/// Turns [ApiController] model-state failures (data annotations, malformed JSON, missing body,
/// missing/invalid headers) into a 400 ErrorResponse with human-readable messages.

public static class ValidationErrorFactory
{
    private const string BodyKey = "body";
    private const string BodyRequired = "Request body is required: {\"name\": string, \"totalSeats\": number}.";
    private const string InvalidValue = "Has an invalid value or the JSON is malformed.";

    public static IActionResult Create(ActionContext context)
    {
        var errors = Normalize(context.ModelState);
        var message = errors.Values.SelectMany(v => v).FirstOrDefault() ?? "The request is invalid.";
        // A JSON parse error on a field is more useful with its field name in the message.
        var firstKey = errors.Keys.FirstOrDefault();
        if (message == InvalidValue && firstKey is not null && firstKey != BodyKey)
            message = $"{firstKey}: {InvalidValue}";

        return new BadRequestObjectResult(
            ErrorResponse.Create(StatusCodes.Status400BadRequest, message, errors));
    }

    private static Dictionary<string, string[]> Normalize(ModelStateDictionary modelState)
    {
        var result = new Dictionary<string, string[]>();
        var hasJsonError = modelState.Keys.Any(k => k.StartsWith('$'));

        foreach (var (rawKey, entry) in modelState)
        {
            if (entry.Errors.Count == 0) continue;

            var isJsonPath = rawKey.StartsWith('$');
            var key = rawKey switch
            {
                "" or "$" or "request" => BodyKey,
                _ when isJsonPath => rawKey.TrimStart('$', '.'),
                _ => ToCamel(rawKey),
            };

            // When the JSON itself is broken, MVC also reports "The request field is required." — drop it.
            if (rawKey == "request" && hasJsonError) continue;

            var messages = entry.Errors.Select(e =>
                isJsonPath ? InvalidValue
                : rawKey is "" or "request" ? BodyRequired
                : string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Is invalid." : e.ErrorMessage);

            result[key] = result.TryGetValue(key, out var existing)
                ? existing.Concat(messages).Distinct().ToArray()
                : messages.Distinct().ToArray();
        }

        return result;
    }

    // "TotalSeats" -> "totalSeats"; header names like "X-Student-Id" are kept verbatim.
    private static string ToCamel(string key) =>
        key.Contains('-') || key.Length == 0 ? key : char.ToLowerInvariant(key[0]) + key[1..];
}
