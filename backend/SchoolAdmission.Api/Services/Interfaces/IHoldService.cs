using SchoolAdmission.Api.Models;

namespace SchoolAdmission.Api.Services.Interfaces;

public interface IHoldService
{
    // Places a HELD hold for a validated 17-digit student id.
    /// <exception cref="Exceptions.NotFoundException">Class does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">Class is full, or the student already has an active hold.</exception>
    Task<Hold> CreateAsync(Guid classId, string studentId, CancellationToken ct = default);

    // Read-only; never mutates the stored status (expiry is derived by the caller via Hold.IsExpired).
    /// <exception cref="Exceptions.NotFoundException">Hold does not exist.</exception>
    Task<Hold> GetAsync(Guid holdId, CancellationToken ct = default);

    // All holds of a student (any status), newest first, with Class loaded for its name.
    Task<IReadOnlyList<Hold>> ListForStudentAsync(string studentId, CancellationToken ct = default);

    // Idempotent: an already CONFIRMED hold is returned unchanged.
    /// <exception cref="Exceptions.NotFoundException">Hold does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">Hold belongs to another student.</exception>
    /// <exception cref="Exceptions.ConflictException">Hold is EXPIRED or CANCELLED.</exception>
    Task<Hold> ConfirmAsync(Guid holdId, string studentId, CancellationToken ct = default);

    // No-op for CANCELLED or EXPIRED holds.
    /// <exception cref="Exceptions.NotFoundException">Hold does not exist.</exception>
    /// <exception cref="Exceptions.ForbiddenException">Hold belongs to another student.</exception>
    /// <exception cref="Exceptions.ConflictException">Hold is CONFIRMED.</exception>
    Task CancelAsync(Guid holdId, string studentId, CancellationToken ct = default);
}
