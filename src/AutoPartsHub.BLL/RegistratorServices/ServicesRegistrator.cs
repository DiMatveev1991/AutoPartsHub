using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPartsHub.BLL.RegistratorServices;

/// <summary>
/// Регистрирует интерфейсы и реализации бизнес-сервисов.
/// </summary>
public static class ServicesRegistrator
{
    /// <summary>
    /// Добавляет бизнес-логику AutoParts Hub с временем жизни одной команды.
    /// </summary>
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        // Presentation Layer зависит от интерфейсов, а не создаёт сервисы через
        // new. Это позволяет одинаково использовать BLL из Telegram и консоли.
        // Scoped выбран из-за Scoped-репозитория и DbContext: одна команда видит
        // один согласованный Unit of Work, но соседние команды не делят контекст.
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }
}
