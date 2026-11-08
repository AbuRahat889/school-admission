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
        var cls = await LockClassAsync(classId, ct)
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

    public async Task<Hold> GetAsync(Guid holdId, CancellationToken ct = default) =>
        await db.Holds.AsNoTracking().FirstOrDefaultAsync(h => h.Id == holdId, ct)
        ?? throw HoldNotFound(holdId);

    public async Task<Hold> ConfirmAsync(Guid holdId, string studentId, CancellationToken ct = default)
    {
        var classId = await CheckOwnershipAsync(holdId, studentId, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockClassAsync(classId, ct);

        // Re-read under the class lock: a concurrent confirm/cancel/create on this class has finished.
        var hold = await db.Holds.FirstOrDefaultAsync(h => h.Id == holdId, ct) ?? throw HoldNotFound(holdId);
        var now = clock.GetUtcNow();

        if (hold.Status == HoldStatus.Confirmed)
            return hold; // Rule 5: idempotent, nothing changes.
        if (hold.Status == HoldStatus.Cancelled)
            throw new ConflictException("This hold was cancelled and can no longer be confirmed.");
        if (hold.IsExpired(now))
            throw new ConflictException("This hold has expired. Please place a new hold.");

        // HELD and not expired. Its seat is already counted as held, so capacity cannot be exceeded.
        hold.Status = HoldStatus.Confirmed;
        hold.ConfirmedAt = now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return hold;
    }

    public async Task CancelAsync(Guid holdId, string studentId, CancellationToken ct = default)
    {
        var classId = await CheckOwnershipAsync(holdId, studentId, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockClassAsync(classId, ct);

        var hold = await db.Holds.FirstOrDefaultAsync(h => h.Id == holdId, ct) ?? throw HoldNotFound(holdId);
        var now = clock.GetUtcNow();

        if (hold.Status == HoldStatus.Confirmed)
            throw new ConflictException("A confirmed admission cannot be cancelled.");

        // Already CANCELLED, or EXPIRED (left as stored HELD so it keeps reading as EXPIRED): no-op.
        if (hold.Status == HoldStatus.Cancelled || hold.IsExpired(now))
            return;

        hold.Status = HoldStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>404 if the hold doesn't exist, 403 if it isn't this student's. Returns its class id.</summary>
    private async Task<Guid> CheckOwnershipAsync(Guid holdId, string studentId, CancellationToken ct)
    {
        var owner = await db.Holds.AsNoTracking()
                        .Where(h => h.Id == holdId)
                        .Select(h => new { h.ClassId, h.StudentId })
                        .FirstOrDefaultAsync(ct)
                    ?? throw HoldNotFound(holdId);
        if (owner.StudentId != studentId)
            throw new ForbiddenException("This hold belongs to another student.");
        return owner.ClassId;
    }

    /// <summary>
    /// Locks the class row for the rest of the transaction (rule 1). Every write path
    /// (create, confirm, cancel) takes this lock first, so they serialize per class.
    /// EF Core 8 has no LINQ operator for FOR UPDATE, so this one statement is raw SQL
    /// (parameterized; not composed further so EF sends it as-is).
    /// </summary>
    private async Task<Class?> LockClassAsync(Guid classId, CancellationToken ct)
    {
        var rows = await db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(ct);
        return rows.FirstOrDefault();
    }

    private static NotFoundException HoldNotFound(Guid holdId) => new($"Hold {holdId} was not found.");
}
