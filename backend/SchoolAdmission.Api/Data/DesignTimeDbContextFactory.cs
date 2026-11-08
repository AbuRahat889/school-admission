using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SchoolAdmission.Api.Data;

/// <summary>
/// Used only by `dotnet ef` so migrations can be generated without a running database
/// or the full app configuration.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? "Host=localhost;Port=5432;Database=school_admission;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<AppDbContext>();
        DbSetup.Configure(options, cs);
        return new AppDbContext(options.Options);
    }
}
