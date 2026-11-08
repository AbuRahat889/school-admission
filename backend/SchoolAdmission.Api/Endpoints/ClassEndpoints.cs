using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Endpoints;

public record CreateClassRequest(string? Name, int? TotalSeats);

public record ClassCreatedResponse(Guid Id, string Name, int TotalSeats);

public record ClassResponse(
    Guid Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);

public static class ClassEndpoints
{
    public const int MaxNameLength = 200;

    public static void MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes", CreateClass);
        app.MapGet("/classes", ListClasses);
        app.MapGet("/classes/{classId:guid}", GetClass);
    }

    private static async Task<IResult> CreateClass(CreateClassRequest? req, AppDbContext db)
    {
        if (req is null)
            return ApiErrors.BadRequest("Request body is required: {\"name\": string, \"totalSeats\": number}.");

        var name = req.Name?.Trim();
        if (string.IsNullOrEmpty(name))
            return ApiErrors.BadRequest("Name is required.");
        if (name.Length > MaxNameLength)
            return ApiErrors.BadRequest($"Name must be at most {MaxNameLength} characters.");
        if (req.TotalSeats is null)
            return ApiErrors.BadRequest("totalSeats is required.");
        if (req.TotalSeats <= 0)
            return ApiErrors.BadRequest("totalSeats must be greater than 0.");

        var entity = new Class
        {
            Id = Guid.NewGuid(),
            Name = name,
            TotalSeats = req.TotalSeats.Value,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Classes.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/classes/{entity.Id}",
            new ClassCreatedResponse(entity.Id, entity.Name, entity.TotalSeats));
    }

    private static async Task<IResult> ListClasses(AppDbContext db)
    {
        var items = await SeatCounts.Query(db, DateTimeOffset.UtcNow)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        return Results.Ok(items.Select(SeatCounts.ToResponse));
    }

    private static async Task<IResult> GetClass(Guid classId, AppDbContext db)
    {
        var item = await SeatCounts.Query(db, DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(x => x.Id == classId);
        return item is null
            ? ApiErrors.NotFound($"Class {classId} was not found.")
            : Results.Ok(SeatCounts.ToResponse(item));
    }
}

/// <summary>
/// Single source of truth for seat counting (rule 2: expiry is derived, never stored).
/// held      = HELD holds with expiresAt > now
/// confirmed = CONFIRMED holds
/// available = total - held - confirmed
/// </summary>
public static class SeatCounts
{
    // Member-init (not a positional ctor) so EF can translate Where/OrderBy on top of the projection.
    public sealed class Row
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public int TotalSeats { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public int Held { get; init; }
        public int Confirmed { get; init; }
    }

    public static IQueryable<Row> Query(AppDbContext db, DateTimeOffset now) =>
        db.Classes.AsNoTracking().Select(c => new Row
        {
            Id = c.Id,
            Name = c.Name,
            TotalSeats = c.TotalSeats,
            CreatedAt = c.CreatedAt,
            Held = c.Holds.Count(h => h.Status == HoldStatus.Held && h.ExpiresAt > now),
            Confirmed = c.Holds.Count(h => h.Status == HoldStatus.Confirmed),
        });

    public static ClassResponse ToResponse(Row r) =>
        new(r.Id, r.Name, r.TotalSeats, r.Held, r.Confirmed,
            Math.Max(0, r.TotalSeats - r.Held - r.Confirmed));
}
