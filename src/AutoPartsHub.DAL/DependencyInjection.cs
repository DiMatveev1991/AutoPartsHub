using AutoPartsHub.Core;
using AutoPartsHub.DAL.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPartsHub.DAL;

/// <summary>
/// Регистрирует зависимости слоя доступа к данным.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Подключает PostgreSQL, репозиторий и инфраструктурные сервисы AutoParts Hub.
    /// </summary>
    public static IServiceCollection AddAutoPartsHubDal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException(
                "Не задана строка подключения ConnectionStrings:PostgreSQL.");

        services.AddDbContext<AutoPartsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AutoPartsDbContext).Assembly.FullName)));

        services.AddScoped<IAutoPartsRepository, AutoPartsRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IOrderNumberGenerator, OrderNumberGenerator>();
        return services;
    }
}
