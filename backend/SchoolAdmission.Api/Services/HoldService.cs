using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Models;
using SchoolAdmission.Api.Services.Exceptions;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Services;

public sealed class HoldService(AppDbContext db, HoldOptions options, TimeProvider clock) : IHoldService
{
    public async Task<Hold> CreateAsync(Guid classId, string studentId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Rule 1: serialize hold creation per class on its row lock, then recount.
        // EF Core 8 has no LINQ operator for FOR UPDATE, so this one statement is raw SQL
        // (parameterized; not composed further so EF sends it as-is).
        var locked = await db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(ct);
        var cls = locked.FirstOrDefault()
                  ?? throw new NotFoundException($"Class {classId} was not found.");

        var now = clock.GetUtcNow();

        // Rule 3: one active (unexpired HELD, or CONFIRMED) hold per student per class.
        var hasActive = await db.Holds.AnyAsync(h =>
            h.ClassId == classId && h.StudentId == studentId &&
            (h.Status == HoldStatus.Confirmed || (h.Status == HoldStatus.Held && h.ExpiresAt > now)), ct);
        if (hasActive)
            throw new ConflictException("You already have an active hold for this class.");

        // An expired HELD row still matches the partial unique index (it can't reference now()),
        // so retire it before inserting the new hold. Done under the class lock.
        await db.Holds
            .Where(h => h.ClassId == classId && h.StudentId == studentId &&
                        h.Status == HoldStatus.Held && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.Cancelled), ct);

        var taken = await db.Holds.CountAsync(h =>
            h.ClassId == classId &&
            (h.Status == HoldStatus.Confirmed || (h.Status == HoldStatus.Held && h.ExpiresAt > now)), ct);
        if (taken >= cls.TotalSeats)
            throw new ConflictException("This class is full. No seats are available right now.");

        var hold = new Hold
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.Held,
            ExpiresAt = now + options.Duration,
            CreatedAt = now,
        };
        db.Holds.Add(hold);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Safety net; the class lock should make this unreachable.
            throw new ConflictException("You already have an active hold for this class.");
        }

        return hold;
    }
}
