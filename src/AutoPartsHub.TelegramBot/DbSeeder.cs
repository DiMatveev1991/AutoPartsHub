using AutoPartsHub.Core;
using AutoPartsHub.DAL.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.TelegramBot;

public static class DbSeeder
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AutoPartsDbContext>();

        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
            await db.Database.MigrateAsync(cancellationToken);

        if (await db.Products.AnyAsync(cancellationToken))
            return;

        var now = DateTimeOffset.UtcNow;
        var filters = new Category("Фильтры", "filters");
        var brakes = new Category("Тормозная система", "brakes");
        await db.Categories.AddRangeAsync([filters, brakes], cancellationToken);

        var oilFilter = new Product(
            filters.Id,
            "21050-22010",
            "Масляный фильтр Toyota",
            "Новый масляный фильтр для бензиновых двигателей Toyota.",
            ProductCondition.New,
            450m,
            15,
            now);
        oilFilter.AddCompatibility("Toyota", "Camry", 2012, 2018, "2.5");

        var airFilter = new Product(
            filters.Id,
            "AF-VAZ-01",
            "Воздушный фильтр ВАЗ",
            "Воздушный фильтр для автомобилей семейства Lada.",
            ProductCondition.New,
            620m,
            8,
            now);
        airFilter.AddCompatibility("Lada", "Vesta", 2015, 2026, null);

        var brakePads = new Product(
            brakes.Id,
            "BP-FORD-02",
            "Передние тормозные колодки Ford",
            "Комплект восстановленных передних тормозных колодок.",
            ProductCondition.Refurbished,
            2300m,
            4,
            now);
        brakePads.AddCompatibility("Ford", "Focus", 2011, 2018, null);

        await db.Products.AddRangeAsync([oilFilter, airFilter, brakePads], cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Добавлены демонстрационные категории и товары");
    }
}
