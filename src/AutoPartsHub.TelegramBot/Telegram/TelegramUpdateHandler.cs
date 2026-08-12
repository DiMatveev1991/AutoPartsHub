using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Преобразует входящие обновления Telegram в вызовы обработчика команд.
/// </summary>
/// <param name="scopeFactory">Фабрика областей зависимостей для обработки сообщений.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class TelegramUpdateHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramUpdateHandler> logger) : IUpdateHandler
{
    /// <summary>
    /// Обрабатывает текстовое сообщение и отправляет пользователю результат команды.
    /// </summary>
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        var message = update.Message;
        if (message?.Text is null)
            return;

        string response;
        try
        {
            // Один update получает отдельный scope и отдельный экземпляр DbContext.
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<BotCommandHandler>();
            var displayName = string.Join(
                ' ',
                new[] { message.From?.FirstName, message.From?.LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = message.From?.Username ?? "Пользователь Telegram";

            response = await handler.HandleAsync(
                message.Chat.Id,
                displayName,
                message.Text,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Технические детали сохраняются в журнале, но пользователю не
            // раскрываются строки подключения, SQL и внутреннее устройство приложения.
            logger.LogError(exception, "Ошибка обработки Telegram-команды");
            response = "Не удалось выполнить команду. Повторите позже.";
        }

        await botClient.SendMessage(
            message.Chat.Id,
            response,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Записывает в журнал необработанную ошибку инфраструктуры Telegram.
    /// </summary>
    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Ошибка Telegram-бота: {Source}", source);
        return Task.CompletedTask;
    }
}
