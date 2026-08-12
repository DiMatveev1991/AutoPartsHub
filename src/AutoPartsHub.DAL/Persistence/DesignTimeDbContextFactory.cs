using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoPartsHub.DAL.Persistence;

/// <summary>
/// Создаёт контекст базы данных для команд <c>dotnet ef</c>.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutoPartsDbContext>
{
    /// <summary>
    /// Создаёт настроенный контекст времени проектирования.
    /// </summary>
    public AutoPartsDbContext CreateDbContext(string[] args)
    {
        // Команды dotnet ef создают DAL без запуска TelegramBot, поэтому не могут
        // использовать его DI-контейнер. Переменная окружения позволяет указать
        // любую БД; запасная строка предназначена только для локального Docker.
        var connectionString = Environment.GetEnvironmentVariable("AUTOPARTS_DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<AutoPartsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AutoPartsDbContext(options);
    }
}
