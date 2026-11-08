using SchoolAdmission.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Listen on 8080 everywhere (local + Docker). ASPNETCORE_URLS still overrides if set.
if (string.IsNullOrEmpty(builder.Configuration["ASPNETCORE_URLS"]) &&
    string.IsNullOrEmpty(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

builder.Services.AddAppDb(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health");

app.Run();

public partial class Program;
