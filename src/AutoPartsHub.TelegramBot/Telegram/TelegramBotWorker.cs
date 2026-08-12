using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Запускает получение обновлений Telegram и регистрирует меню команд бота.
/// </summary>
/// <param name="options">Настройки Telegram.</param>
/// <param name="handler">Обработчик входящих обновлений.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class TelegramBotWorker(
    IOptions<TelegramOptions> options,
    TelegramUpdateHandler handler,
    ILogger<TelegramBotWorker> logger) : BackgroundService
{
    /// <summary>
    /// Запускает long polling и ожидает завершения приложения.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.EnablePolling || string.IsNullOrWhiteSpace(options.Value.BotToken))
        {
            logger.LogInformation("Telegram polling отключён");
            return;
        }

        var client = new TelegramBotClient(options.Value.BotToken);
        await client.SetMyCommands(
            [
                new BotCommand { Command = "catalog", Description = "Показать каталог" },
                new BotCommand { Command = "find", Description = "Найти деталь" },
                new BotCommand { Command = "cart", Description = "Открыть корзину" },
                new BotCommand { Command = "orders", Description = "Мои заказы" },
                new BotCommand { Command = "status", Description = "Проверить заказ" },
                new BotCommand { Command = "help", Description = "Справка" }
            ],
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
