using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Запускает получение обновлений Telegram и регистрирует меню команд бота.
/// </summary>
/// <param name="options">Настройки Telegram.</param>
/// <param name="clientProvider">Общий Telegram-клиент с настройками прокси.</param>
/// <param name="handler">Обработчик входящих обновлений.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class TelegramBotWorker(
    IOptions<TelegramOptions> options,
    TelegramBotClientProvider clientProvider,
    TelegramUpdateHandler handler,
    ILogger<TelegramBotWorker> logger) : BackgroundService
{
    /// <summary>
    /// Запускает long polling и ожидает завершения приложения.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = clientProvider.Client;
        if (!options.Value.EnablePolling || client is null)
        {
            logger.LogInformation("Telegram polling отключён");
            return;
        }

        if (clientProvider.UsesProxy)
        {
            logger.LogInformation(
                "Telegram использует прокси {Proxy}",
                clientProvider.ProxyDisplayName);
        }

        await client.SetMyCommands(
            TelegramMenu.CreateBotCommands(),
            cancellationToken: stoppingToken);

        // DropPendingUpdates защищает от повторного выполнения старых команд после
        // перезапуска — особенно оформления заказа и административных операций.
        // StartReceiving запускает внутренний цикл; жизненный цикл worker удерживается Task.Delay ниже.
        client.StartReceiving(
            handler,
            new ReceiverOptions { DropPendingUpdates = true },
            stoppingToken);

        var me = await client.GetMe(stoppingToken);
        logger.LogInformation("Telegram-бот @{BotName} запущен", me.Username);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
