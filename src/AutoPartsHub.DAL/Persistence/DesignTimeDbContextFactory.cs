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
        // использовать его DI-контейнер. Строка передаётся только через окружение,
        // чтобы пароль не хранился в исходном коде.
        var connectionString = Environment.GetEnvironmentVariable("AUTOPARTS_DB_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Не задана переменная окружения AUTOPARTS_DB_CONNECTION_STRING.");
        }
        var options = new DbContextOptionsBuilder<AutoPartsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AutoPartsDbContext(options);
    }
}
