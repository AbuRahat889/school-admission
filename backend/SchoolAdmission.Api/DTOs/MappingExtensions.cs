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

    public static HoldResponse ToResponse(this Hold h, DateTimeOffset now) =>
        new(h.Id, h.ClassId, h.ToApiStatus(now), h.ExpiresAt);

    public static HoldConfirmedResponse ToConfirmedResponse(this Hold h) =>
        new(h.Id, h.Status.ToApiString());

    public const string ExpiredStatus = "EXPIRED";

    /// <summary>Status as seen by clients: stored status, except a lapsed HELD hold reads as EXPIRED.</summary>
    public static string ToApiStatus(this Hold h, DateTimeOffset now) =>
        h.IsExpired(now) ? ExpiredStatus : h.Status.ToApiString();

    /// <summary>Wire representation of the stored status (matches the DB values).</summary>
    public static string ToApiString(this HoldStatus status) => status switch
    {
        HoldStatus.Held => "HELD",
        HoldStatus.Confirmed => "CONFIRMED",
        HoldStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };
}
