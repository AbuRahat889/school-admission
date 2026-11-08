namespace SchoolAdmission.Api.Models;


/// Read model: a class plus its live seat counts. Expiry is derived (CLAUDE.md rule 2),
/// so HeldSeats only counts HELD holds whose expiresAt is still in the future.

public sealed class ClassSeatSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public int TotalSeats { get; init; }
    public int HeldSeats { get; init; }
    public int ConfirmedSeats { get; init; }

    public int AvailableSeats => Math.Max(0, TotalSeats - HeldSeats - ConfirmedSeats);
}
