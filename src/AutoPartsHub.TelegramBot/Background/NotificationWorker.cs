using AutoPartsHub.BLL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.TelegramBot.Background;

/// <summary>
/// Периодически создаёт и отправляет уведомления по товарным подпискам.
/// </summary>
/// <param name="scopeFactory">Фабрика областей зависимостей фоновой обработки.</param>
/// <param name="logger">Журнал приложения.</param>
public sealed class NotificationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    /// <summary>
    /// Запускает обработку сразу после старта и повторяет её каждую минуту.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка фоновой обработки уведомлений");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// Выполняет один цикл подготовки и отправки уведомлений.
    /// </summary>
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<SubscriptionService>();
        var created = await service.PrepareTriggeredNotificationsAsync(cancellationToken);
        var sent = await service.SendPendingAsync(cancellationToken);
        if (created > 0 || sent > 0)
            logger.LogInformation("Создано уведомлений: {Created}; обработано: {Sent}", created, sent);
    }
}
