using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Hold> Holds => Set<Hold>();

    // Stored as the exact strings used by the API contract.
    private static readonly ValueConverter<HoldStatus, string> StatusConverter = new(
        v => v == HoldStatus.Held ? "HELD" : v == HoldStatus.Confirmed ? "CONFIRMED" : "CANCELLED",
        v => v == "HELD" ? HoldStatus.Held : v == "CONFIRMED" ? HoldStatus.Confirmed : HoldStatus.Cancelled);

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Class>(e =>
        {
            e.ToTable("classes", t => t.HasCheckConstraint("ck_classes_total_seats_positive", "total_seats > 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TotalSeats).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<Hold>(e =>
        {
            e.ToTable("holds", t =>
            {
                t.HasCheckConstraint("ck_holds_status", "status IN ('HELD','CONFIRMED','CANCELLED')");
                t.HasCheckConstraint("ck_holds_student_id_format", "student_id ~ '^[0-9]{17}$'");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(17).IsFixedLength().IsRequired();
            e.Property(x => x.Status).HasConversion(StatusConverter).HasMaxLength(16).IsRequired();
            e.Property(x => x.ExpiresAt).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.Class)
             .WithMany(c => c.Holds)
             .HasForeignKey(x => x.ClassId)
             .OnDelete(DeleteBehavior.Cascade);

            // Rule 3: at most one active (HELD or CONFIRMED) hold per student per class.
            e.HasIndex(x => new { x.StudentId, x.ClassId })
             .IsUnique()
             .HasFilter("status IN ('HELD','CONFIRMED')")
             .HasDatabaseName("ux_holds_active_student_class");

            // Seat counting per class, and GET /me/holds.
            e.HasIndex(x => new { x.ClassId, x.Status });
            e.HasIndex(x => x.StudentId);
        });
    }
}
