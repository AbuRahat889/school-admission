using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api.Data;

public static class DbSetup
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

    /// <summary>
    /// Reads the connection string from the env var <c>ConnectionStrings__Default</c>
    /// (standard .NET config binding, also settable via appsettings). Fails fast if missing.
    /// </summary>
    public static IServiceCollection AddAppDb(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                "Missing connection string. Set env var ConnectionStrings__Default, e.g. " +
                "'Host=postgres;Port=5432;Database=school_admission;Username=postgres;Password=postgres'.");

        return services.AddDbContext<AppDbContext>(o => Configure(o, cs));
    }

    /// <summary>
    /// Applies pending EF migrations at startup. Retries briefly in case the DB accepts
    /// connections a moment after its healthcheck passes.
    /// </summary>
    public static async Task MigrateAsync(IServiceProvider services, ILogger logger, int attempts = 10)
    {
        for (var i = 1; ; i++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied.");
                return;
            }
            catch (Exception ex) when (i < attempts)
            {
                logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed, retrying in 2s", i, attempts);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
