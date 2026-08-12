using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoPartsHub.DAL.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutoPartsDbContext>
{
    public AutoPartsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUTOPARTS_DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<AutoPartsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AutoPartsDbContext(options);
    }
}
