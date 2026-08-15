using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Отправляет уведомления через Telegram или записывает их в журнал без токена.
/// </summary>
/// <param name="options">Настройки Telegram.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class TelegramNotificationSender(
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationSender> logger) : INotificationSender
{
    /// <summary>
    /// Отправляет уведомление в Telegram-чат пользователя.
    /// </summary>
    public async Task SendAsync(
        User user,
        Notification notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.BotToken))
        {
            // В демонстрационном режиме уведомления видны в журнале без реального Telegram API.
            logger.LogInformation(
                "Уведомление для чата {ChatId}: {Text}",
                user.TelegramChatId,
                notification.Text);
            return;
        }

        var client = new TelegramBotClient(options.Value.BotToken);
        await client.SendMessage(
            user.TelegramChatId,
            notification.Text,
            cancellationToken: cancellationToken);
    }
}
