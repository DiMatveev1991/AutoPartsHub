using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения уведомлений.</summary>
public interface INotificationRepository
{
    /// <summary>Возвращает историю уведомлений пользователя.</summary>
    Task<IReadOnlyCollection<Notification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Возвращает ожидающие отправки уведомления.</summary>
    Task<IReadOnlyCollection<Notification>> GetPendingAsync(
        CancellationToken cancellationToken);

    /// <summary>Добавляет уведомление в контекст хранения.</summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
}
