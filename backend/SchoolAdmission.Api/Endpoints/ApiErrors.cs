namespace SchoolAdmission.Api.Endpoints;

/// <summary>
/// All error responses are RFC 7807 problem+json with a human-readable <c>detail</c>
/// that the UI can show as-is.
/// </summary>
public static class ApiErrors
{
    public static IResult BadRequest(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: detail);

    public static IResult NotFound(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: detail);

    public static IResult Conflict(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: detail);
}
