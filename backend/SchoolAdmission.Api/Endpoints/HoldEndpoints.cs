using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Endpoints;

public record HoldCreatedResponse(Guid HoldId, Guid ClassId, string StudentId, string Status, DateTimeOffset ExpiresAt);

public static class HoldEndpoints
{
    public static void MapHoldEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes/{classId:guid}/holds", CreateHold);
    }

    private static async Task<IResult> CreateHold(
        Guid classId, HttpRequest request, AppDbContext db, HoldOptions options, TimeProvider clock)
    {
        if (!StudentId.TryRead(request, out var studentId, out var error))
            return error!;

        await using var tx = await db.Database.BeginTransactionAsync();

        // Rule 1: serialize all hold creation for this class on its row lock, then recount.
        // Not composed further, so EF sends this SQL as-is.
        var locked = await db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync();
        var cls = locked.FirstOrDefault();
        if (cls is null)
            return ApiErrors.NotFound($"Class {classId} was not found.");

        var now = clock.GetUtcNow();

        // Rule 3: one active (HELD and not expired, or CONFIRMED) hold per student per class.
        var hasActive = await db.Holds.AnyAsync(h =>
            h.ClassId == classId && h.StudentId == studentId &&
            (h.Status == HoldStatus.Confirmed || (h.Status == HoldStatus.Held && h.ExpiresAt > now)));
        if (hasActive)
            return ApiErrors.Conflict("You already have an active hold for this class.");

        // An expired HELD row still matches the partial unique index (it can't reference now()),
        // so retire it before inserting the new hold. Done under the class lock.
        await db.Holds
            .Where(h => h.ClassId == classId && h.StudentId == studentId &&
                        h.Status == HoldStatus.Held && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.Cancelled));

        var taken = await db.Holds.CountAsync(h =>
            h.ClassId == classId &&
            (h.Status == HoldStatus.Confirmed || (h.Status == HoldStatus.Held && h.ExpiresAt > now)));
        if (taken >= cls.TotalSeats)
            return ApiErrors.Conflict("This class is full. No seats are available right now.");

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
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Safety net; the class lock should make this unreachable.
            return ApiErrors.Conflict("You already have an active hold for this class.");
        }

        return Results.Created($"/holds/{hold.Id}",
            new HoldCreatedResponse(hold.Id, hold.ClassId, hold.StudentId, "HELD", hold.ExpiresAt));
    }
}
