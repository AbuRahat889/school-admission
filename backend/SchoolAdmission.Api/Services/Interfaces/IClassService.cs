using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Models;

namespace SchoolAdmission.Api.Services.Interfaces;

public interface IClassService
{
    Task<Class> CreateAsync(CreateClassRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ClassSeatSummary>> ListAsync(CancellationToken ct = default);

    /// <exception cref="Exceptions.NotFoundException">Class does not exist.</exception>
    Task<ClassSeatSummary> GetAsync(Guid classId, CancellationToken ct = default);
}
