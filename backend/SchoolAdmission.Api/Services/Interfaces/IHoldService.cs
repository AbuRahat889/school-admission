using SchoolAdmission.Api.Models;

namespace SchoolAdmission.Api.Services.Interfaces;

public interface IHoldService
{
    // Places a HELD hold for a validated 17 - digit student id.</summary>
    /// <exception cref="Exceptions.NotFoundException">Class does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">Class is full, or the student already has an active hold.</exception>
   Task<Hold> CreateAsync(Guid classId, string studentId, CancellationToken ct = default);
}
