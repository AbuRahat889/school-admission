using SchoolAdmission.Api.Models;

namespace SchoolAdmission.Api.DTOs;

/// <summary>Entity/read-model -> DTO mapping. Kept explicit on purpose (no AutoMapper).</summary>
public static class MappingExtensions
{
    public static ClassCreatedResponse ToCreatedResponse(this Class c) =>
        new(c.Id, c.Name, c.TotalSeats);

    public static ClassResponse ToResponse(this ClassSeatSummary s) =>
        new(s.Id, s.Name, s.TotalSeats, s.HeldSeats, s.ConfirmedSeats, s.AvailableSeats);

    public static HoldCreatedResponse ToCreatedResponse(this Hold h) =>
        new(h.Id, h.ClassId, h.StudentId, h.Status.ToApiString(), h.ExpiresAt);

    /// <summary>Wire representation of the stored status (matches the DB values).</summary>
    public static string ToApiString(this HoldStatus status) => status switch
    {
        HoldStatus.Held => "HELD",
        HoldStatus.Confirmed => "CONFIRMED",
        HoldStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
