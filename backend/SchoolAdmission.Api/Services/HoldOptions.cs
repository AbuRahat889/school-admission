namespace SchoolAdmission.Api.Services;

public sealed class HoldOptions
{
    public const int DefaultDurationSeconds = 600;

    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(DefaultDurationSeconds);

    /// <summary>Reads HOLD_DURATION_SECONDS (default 600). Invalid values fail startup loudly.</summary>
    public static HoldOptions FromConfiguration(IConfiguration config)
    {
        var raw = config["HOLD_DURATION_SECONDS"];
        if (string.IsNullOrWhiteSpace(raw))
            return new HoldOptions();

        if (!int.TryParse(raw, out var seconds) || seconds <= 0)
            throw new InvalidOperationException(
                $"HOLD_DURATION_SECONDS must be a positive integer number of seconds, got '{raw}'.");

        return new HoldOptions { Duration = TimeSpan.FromSeconds(seconds) };
    }
}
