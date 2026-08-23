using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Отправляет уведомления через Telegram или записывает их в журнал без токена.
/// </summary>
/// <param name="clientProvider">Общий Telegram-клиент с настройками прокси.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class TelegramNotificationSender(
    TelegramBotClientProvider clientProvider,
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
        var client = clientProvider.Client;
        if (client is null)
        {
            // В демонстрационном режиме уведомления видны в журнале без реального Telegram API.
            logger.LogInformation(
                "Уведомление для чата {ChatId}: {Text}",
                user.TelegramChatId,
                notification.Text);
            return;
        }

        await client.SendMessage(
            user.TelegramChatId,
            notification.Text,
            cancellationToken: cancellationToken);
    }
}
