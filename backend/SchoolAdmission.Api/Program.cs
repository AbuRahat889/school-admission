using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Listen on 8080 everywhere (local + Docker). ASPNETCORE_URLS still overrides if set.
if (string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]) &&
    string.IsNullOrEmpty(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

builder.Services.AddAppDb(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(HoldOptions.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await DbSetup.MigrateAsync(app.Services, app.Logger);

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health");

app.MapClassEndpoints();
app.MapHoldEndpoints();

app.Run();

public partial class Program;
