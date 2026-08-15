using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.TelegramBot.Background;
using AutoPartsHub.TelegramBot.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoPartsHub.TelegramBot.Registrator;

/// <summary>
/// Регистрирует Telegram-, консольный и фоновый интерфейсы приложения.
/// </summary>
public static class BotRegistrator
{
    /// <summary>
    /// Добавляет обработчики команд, канал уведомлений и фоновые службы бота.
    /// </summary>
    public static IServiceCollection AddBotPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options отделяет классы Telegram-слоя от строковых ключей IConfiguration.
        services.Configure<TelegramOptions>(
            configuration.GetSection(TelegramOptions.SectionName));

        // Обработчик использует Scoped бизнес-сервисы и создаётся на одну команду.
        // Он общий для Telegram и консоли, поэтому разбор команд не дублируется.
        services.AddScoped<BotCommandHandler>();

        // Отправщик и update handler не удерживают DbContext и безопасно
        // переиспользуются между командами.
        services.AddSingleton<INotificationSender, TelegramNotificationSender>();
        services.AddSingleton<TelegramUpdateHandler>();

        // Hosted services являются Singleton и создают собственные scopes внутри.
        services.AddHostedService<TelegramBotWorker>();
        services.AddHostedService<ConsoleBotWorker>();
        services.AddHostedService<NotificationWorker>();
        return services;
    }
}
