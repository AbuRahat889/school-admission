using System.ComponentModel.DataAnnotations;

namespace SchoolAdmission.Api.DTOs;

public sealed class CreateClassRequest
{
    public const int MaxNameLength = 200;

    // [Required] on a string also rejects empty and whitespace-only values.
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(MaxNameLength, ErrorMessage = "Name must be at most 200 characters.")]
    public string? Name { get; init; }

    [Required(ErrorMessage = "totalSeats is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "totalSeats must be greater than 0.")]
    public int? TotalSeats { get; init; }
}

// POST /classes -> 201 {id,name,totalSeats}</ summary >
public sealed record ClassCreatedResponse(Guid Id, string Name, int TotalSeats);

// GET / classes, GET / classes /{ classId} -> { id,name,totalSeats,heldSeats,confirmedSeats,availableSeats}</ summary >
public sealed record ClassResponse(
    Guid Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);
