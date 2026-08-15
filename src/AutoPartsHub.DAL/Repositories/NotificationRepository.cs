using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение уведомлений через EF Core.</summary>
internal sealed class NotificationRepository(AutoPartsDbContext db) : INotificationRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Notification>> GetPendingAsync(
        CancellationToken cancellationToken) =>
        await db.Notifications
            .Include(item => item.User)
            .Where(item => item.Status == NotificationStatus.Pending)
            .OrderBy(item => item.CreatedAt)
            // Пакет из ста записей ограничивает память и время одного цикла worker-а.
            .Take(100)
            .ToArrayAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        await db.Notifications.AddAsync(notification, cancellationToken);
}
