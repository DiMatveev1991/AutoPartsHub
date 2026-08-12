using AutoPartsHub.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AutoPartsHub.TelegramBot.Telegram;

public sealed class TelegramNotificationSender(
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationSender> logger) : INotificationSender
{
    public async Task SendAsync(
        User user,
        Notification notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.BotToken))
        {
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
