namespace SchoolAdmission.Api.Domain;

/// <summary>
/// Stored statuses only. EXPIRED is never stored: a HELD hold whose ExpiresAt is in the past
/// is treated as expired at read time (CLAUDE.md rule 2).
/// </summary>
public enum HoldStatus
{
    Held,
    Confirmed,
    Cancelled,
}

public class Hold
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    /// <summary>Exactly 17 digits (validated at the API boundary and by a DB check constraint).</summary>
    public string StudentId { get; set; } = string.Empty;

    public HoldStatus Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}
