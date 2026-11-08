namespace SchoolAdmission.Api.Services.Exceptions;


/// Expected, client-caused failures. The error middleware turns these into an ErrorResponse
/// with the given status code; the message is shown to the user as-is.

public abstract class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : AppException(StatusCodes.Status404NotFound, message);

public sealed class ConflictException(string message) : AppException(StatusCodes.Status409Conflict, message);

public sealed class BadRequestException(string message) : AppException(StatusCodes.Status400BadRequest, message);
