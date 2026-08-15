using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.DAL.Repositories;
using AutoPartsHub.DAL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPartsHub.DAL.Registrator;

/// <summary>
/// Регистрирует зависимости слоя доступа к данным.
/// </summary>
public static class RepositoryRegistrator
{
    /// <summary>
    /// Подключает PostgreSQL, репозитории и инфраструктурные сервисы AutoParts Hub.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DAL получает строку подключения на границе приложения. Ни BLL, ни
        // Telegram-обработчики не знают имя ключа и не работают с IConfiguration.
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:PostgreSQL.");

        // AddDbContext по умолчанию регистрирует контекст как Scoped. DbContext
        // представляет одну единицу работы и не является потокобезопасным.
        services.AddDbContext<AutoPartsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                // Миграции находятся в DAL, а запускаемым проектом является TelegramBot.
                // Явное указание сборки не даёт EF Core искать их в startup-проекте.
                npgsql.MigrationsAssembly(typeof(AutoPartsDbContext).Assembly.FullName)));

        // Как в DeliveryApp, каждый репозиторий имеет небольшой интерфейс по своей
        // области. Все реализации Scoped и получают один и тот же Scoped DbContext,
        // поэтому UnitOfWork сохраняет их изменения атомарно в рамках команды.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Реализации не содержат изменяемого состояния: SystemClock читает системное
        // время, а генератор использует локальные значения и Guid, поэтому Singleton безопасен.
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IOrderNumberGenerator, OrderNumberGenerator>();
        return services;
    }
}
