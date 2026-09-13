using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenLedg.Infrastructure.Persistence;

public sealed class OlgpDbContextFactory : IDesignTimeDbContextFactory<OlgpDbContext>
{
    public OlgpDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Olgp")
            ?? "Host=localhost;Database=openledg;Username=openledg_owner";
        return new OlgpDbContext(new DbContextOptionsBuilder<OlgpDbContext>()
            .UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "olgp")).Options);
    }
}
