using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OpenLedg.Api;
using OpenLedg.Application.Abstractions;
using OpenLedg.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
var connection = builder.Configuration.GetConnectionString("Olgp")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Olgp before starting the application.");
builder.Services.AddDbContext<OlgpDbContext>(options => options.UseNpgsql(connection,
    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "olgp")));
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddCheck<OlgpHealthCheck>("olgp", tags: ["ready"]);

var app = builder.Build();
if (args.Contains("--migrate", StringComparer.Ordinal))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<OlgpDbContext>().Database.MigrateAsync();
    return;
}

app.UseExceptionHandler();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapGet("/", () => Results.Ok(new { service = "OpenLedg", component = "OLGP", status = "foundation" }));
await app.RunAsync();
