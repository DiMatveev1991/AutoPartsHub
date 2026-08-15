using AutoPartsHub.Models;
using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.TelegramBot;

/// <summary>
/// Применяет миграции и добавляет демонстрационные данные в пустую базу.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Подготавливает схему базы данных и начальный каталог.
    /// </summary>
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // DbContext зарегистрирован как Scoped, поэтому его нельзя получать напрямую
        // из корневого IServiceProvider. Временный scope корректно освобождает контекст.
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AutoPartsDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        // Автомиграции удобны для учебного проекта и локального запуска.
        // В окружении с отдельным этапом деплоя их можно отключить настройкой
        // Database:ApplyMigrationsOnStartup и выполнить через dotnet ef заранее.
        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
            await db.Database.MigrateAsync(cancellationToken);

        // Seeder заполняет только полностью пустой каталог. Это не позволяет ему
        // перезаписать пользовательские данные или создать категории с тем же slug.
        if (await db.Products.AnyAsync(cancellationToken) ||
            await db.Categories.AnyAsync(cancellationToken))
            return;

        var now = clock.UtcNow;
        // В отличие от пользовательских команд, seeder содержит заранее проверенные
        // константы и не является бизнес-сценарием. Поэтому он заполняет POCO-модели
        // напрямую; все внешние данные по-прежнему проходят через правила BLL.
        var filters = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Фильтры",
            Slug = "filters"
        };
        var brakes = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Тормозная система",
            Slug = "brakes"
        };
        await db.Categories.AddRangeAsync([filters, brakes], cancellationToken);

        var oilFilter = CreateProduct(
            filters.Id,
            "21050-22010",
            "Масляный фильтр Toyota",
            "Новый масляный фильтр для бензиновых двигателей Toyota.",
            ProductCondition.New,
            450m,
            15,
            now,
            ("Toyota", "Camry", 2012, 2018, "2.5"));

        var airFilter = CreateProduct(
            filters.Id,
            "AF-VAZ-01",
            "Воздушный фильтр ВАЗ",
            "Воздушный фильтр для автомобилей семейства Lada.",
            ProductCondition.New,
            620m,
            8,
            now,
            ("Lada", "Vesta", 2015, 2026, null));

        var brakePads = CreateProduct(
            brakes.Id,
            "BP-FORD-02",
            "Передние тормозные колодки Ford",
            "Комплект восстановленных передних тормозных колодок.",
            ProductCondition.Refurbished,
            2300m,
            4,
            now,
            ("Ford", "Focus", 2011, 2018, null));

        await db.Products.AddRangeAsync([oilFilter, airFilter, brakePads], cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Добавлены демонстрационные категории и товары");
    }

    /// <summary>
    /// Собирает одну заранее проверенную демонстрационную карточку товара.
    /// </summary>
    private static Product CreateProduct(
        Guid categoryId,
        string article,
        string name,
        string description,
        ProductCondition condition,
        decimal price,
        int stock,
        DateTimeOffset now,
        (string Make, string Model, int YearFrom, int YearTo, string? Engine) compatibility)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Article = article,
            Name = name,
            Description = description,
            Condition = condition,
            Price = price,
            Stock = stock,
            IsActive = true,
            ConcurrencyToken = Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now
        };
        product.Compatibilities.Add(new ProductCompatibility
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Make = compatibility.Make,
            Model = compatibility.Model,
            YearFrom = compatibility.YearFrom,
            YearTo = compatibility.YearTo,
            Engine = compatibility.Engine
        });
        return product;
    }
}
