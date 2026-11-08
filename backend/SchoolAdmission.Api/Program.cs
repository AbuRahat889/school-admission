using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Api.Services;
using SchoolAdmission.Api.Services.Interfaces;

public partial class Program
{
    private const string FrontendCorsPolicy = "frontend";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Listen on 8080 everywhere (local + Docker). ASPNETCORE_URLS still overrides if set.
        if (string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]) &&
            string.IsNullOrEmpty(builder.Configuration["urls"]))
        {
            builder.WebHost.UseUrls("http://0.0.0.0:8080");
        }

        builder.Services.AddAppDb(builder.Configuration);
        builder.Services.AddSingleton(HoldOptions.FromConfiguration(builder.Configuration));
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IClassService, ClassService>();
        builder.Services.AddScoped<IHoldService, HoldService>();

        builder.Services
            .AddControllers()
            .ConfigureApiBehaviorOptions(o =>
            {
                // All 4xx bodies are ErrorResponse: validation via the factory, the rest via ErrorHandlingMiddleware.
                o.InvalidModelStateResponseFactory = ValidationErrorFactory.Create;
                o.SuppressMapClientErrors = true;
            });
        builder.Services.AddCors(o => o.AddPolicy(FrontendCorsPolicy, p => p
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()   // X-Student-Id, Content-Type
            .AllowAnyMethod()   // incl. DELETE and preflight
            .WithExposedHeaders("Location")));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        await DbSetup.MigrateAsync(app.Services, app.Logger);

        // CORS first so preflights short-circuit and error responses carry CORS headers too.
        app.UseCors(FrontendCorsPolicy);
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();

        app.Run();
    }
}

