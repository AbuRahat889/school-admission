using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Models;
using SchoolAdmission.Api.Services.Exceptions;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Services;

public sealed class ClassService(AppDbContext db, TimeProvider clock) : IClassService
{
    public async Task<Class> CreateAsync(CreateClassRequest request, CancellationToken ct = default)
    {
        // Shape/range already validated by data annotations on the DTO.
        var entity = new Class
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            TotalSeats = request.TotalSeats!.Value,
            CreatedAt = clock.GetUtcNow(),
        };
        db.Classes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<IReadOnlyList<ClassSeatSummary>> ListAsync(CancellationToken ct = default) =>
        await WithSeatCounts(db.Classes.OrderBy(c => c.CreatedAt), clock.GetUtcNow())
            .ToListAsync(ct);

    public async Task<ClassSeatSummary> GetAsync(Guid classId, CancellationToken ct = default) =>
        await WithSeatCounts(db.Classes.Where(c => c.Id == classId), clock.GetUtcNow())
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException($"Class {classId} was not found.");

    /// <summary>
    /// Single source of truth for seat counts. Filter/order the classes first, then project:
    /// held = HELD with expiresAt > now, confirmed = CONFIRMED.
    /// </summary>
    private static IQueryable<ClassSeatSummary> WithSeatCounts(IQueryable<Class> classes, DateTimeOffset now) =>
        classes.AsNoTracking().Select(c => new ClassSeatSummary
        {
            Id = c.Id,
            Name = c.Name,
            TotalSeats = c.TotalSeats,
            HeldSeats = c.Holds.Count(h => h.Status == HoldStatus.Held && h.ExpiresAt > now),
            ConfirmedSeats = c.Holds.Count(h => h.Status == HoldStatus.Confirmed),
        });
}
