namespace SchoolAdmission.Api.Endpoints;

public static class StudentId
{
    public const string HeaderName = "X-Student-Id";
    public const int Length = 17;

    /// <summary>
    /// Validates the X-Student-Id header: present, single value, exactly 17 ASCII digits.
    /// Returns null + a 400 result on failure.
    /// </summary>
    public static bool TryRead(HttpRequest request, out string studentId, out IResult? error)
    {
        studentId = "";
        error = null;

        var values = request.Headers[HeaderName];
        if (values.Count == 0 || string.IsNullOrEmpty(values[0]))
        {
            error = ApiErrors.BadRequest($"{HeaderName} header is required.");
            return false;
        }
        if (values.Count > 1)
        {
            error = ApiErrors.BadRequest($"{HeaderName} header must be sent once.");
            return false;
        }

        var value = values[0]!;
        if (value.Length != Length || !value.All(char.IsAsciiDigit))
        {
            error = ApiErrors.BadRequest($"{HeaderName} must be exactly {Length} digits.");
            return false;
        }

        studentId = value;
        return true;
    }
}
