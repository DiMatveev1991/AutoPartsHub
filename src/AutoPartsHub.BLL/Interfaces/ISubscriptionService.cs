using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает товарные подписки и обработку уведомлений.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>Создаёт подписку пользователя на товар.</summary>
    Task SubscribeAsync(
        Guid userId,
        SubscribeRequest request,
        CancellationToken cancellationToken);

    /// <summary>Возвращает историю уведомлений пользователя.</summary>
    Task<IReadOnlyCollection<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Создаёт уведомления для сработавших подписок.</summary>
    Task<int> PrepareTriggeredNotificationsAsync(CancellationToken cancellationToken);

    /// <summary>Отправляет ожидающие уведомления.</summary>
    Task<int> SendPendingAsync(CancellationToken cancellationToken);
}
