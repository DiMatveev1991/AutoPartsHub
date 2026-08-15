using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение уведомлений через EF Core.</summary>
internal sealed class NotificationRepository(AutoPartsDbContext db) : INotificationRepository
{
    /// <summary>
    /// Возвращает историю уведомлений пользователя от новых к старым без отслеживания сущностей.
    /// История предназначена только для чтения, поэтому Change Tracker здесь не нужен.
    /// </summary>
    public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);

    /// <summary>
    /// Загружает ограниченную пачку ожидающих уведомлений вместе с пользователями для фоновой отправки.
    /// Сущности отслеживаются, чтобы после успешной отправки сохранить новый статус тем же Unit of Work.
    /// </summary>
    public async Task<IReadOnlyCollection<Notification>> GetPendingAsync(
        CancellationToken cancellationToken) =>
        await db.Notifications
            .Include(item => item.User)
            .Where(item => item.Status == NotificationStatus.Pending)
            .OrderBy(item => item.CreatedAt)
            // Пакет из ста записей ограничивает память и время одного цикла worker-а.
            .Take(100)
            .ToArrayAsync(cancellationToken);

    /// <summary>
    /// Добавляет уведомление в текущий Change Tracker без отдельного commit.
    /// Отложенное сохранение не допускает появления уведомления, если породившая его бизнес-операция откатилась.
    /// </summary>
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        await db.Notifications.AddAsync(notification, cancellationToken);
}
