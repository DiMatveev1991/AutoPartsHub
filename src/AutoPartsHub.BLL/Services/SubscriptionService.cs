using AutoPartsHub.BLL;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.BLL.Services;

/// <summary>
/// Управляет товарными подписками и доставкой созданных уведомлений.
/// </summary>
/// <param name="catalog">Хранилище каталога.</param>
/// <param name="subscriptions">Хранилище товарных подписок.</param>
/// <param name="notifications">Хранилище уведомлений.</param>
/// <param name="unitOfWork">Граница сохранения изменений.</param>
/// <param name="notificationSender">Канал отправки уведомлений.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class SubscriptionService(
    ICatalogRepository catalog,
    ISubscriptionRepository subscriptions,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    INotificationSender notificationSender,
    IClock clock) : ISubscriptionService
{
    /// <summary>
    /// Создаёт уникальную активную подписку пользователя на товар.
    /// </summary>
    public async Task SubscribeAsync(
        Guid userId,
        SubscribeRequest request,
        CancellationToken cancellationToken)
    {
        if (await catalog.FindProductAsync(request.ProductId, cancellationToken) is null)
            throw new NotFoundException("Товар не найден.");
        if (await subscriptions.ActiveExistsAsync(
                userId,
                request.ProductId,
                request.Type,
                cancellationToken))
            throw new ConflictException("Такая подписка уже существует.");

        await subscriptions.AddAsync(
            SubscriptionRules.Create(
                userId,
                request.ProductId,
                request.Type,
                request.TargetPrice,
                clock.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает историю уведомлений пользователя.
    /// </summary>
    public async Task<IReadOnlyCollection<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var items = await notifications.GetByUserAsync(userId, cancellationToken);
        return items.Select(item => new NotificationDto(
            item.Id,
            item.Type,
            item.Text,
            item.Status,
            item.CreatedAt,
            item.SentAt)).ToArray();
    }

    /// <summary>
    /// Создаёт уведомления для всех сработавших подписок.
    /// </summary>
    public async Task<int> PrepareTriggeredNotificationsAsync(CancellationToken cancellationToken)
    {
        var triggered = await subscriptions.GetTriggeredAsync(cancellationToken);
        foreach (var subscription in triggered)
        {
            var product = subscription.Product
                ?? throw new InvalidOperationException("Товар подписки не загружен.");
            var text = subscription.Type == SubscriptionType.BackInStock
                ? $"Товар «{product.Name}» ({product.Article}) снова в наличии."
                : $"Цена товара «{product.Name}» снизилась до {product.Price:F2}.";

            await notifications.AddAsync(
                SubscriptionRules.CreateNotification(
                    subscription.UserId,
                    subscription.Type.ToString(),
                    text,
                    clock.UtcNow),
                cancellationToken);
            SubscriptionRules.Complete(subscription);
        }

        // Подписка завершается и уведомление добавляется одним сохранением.
        if (triggered.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        return triggered.Count;
    }

    /// <summary>
    /// Отправляет ожидающие уведомления и фиксирует результат каждой попытки.
    /// </summary>
    public async Task<int> SendPendingAsync(CancellationToken cancellationToken)
    {
        var pending = await notifications.GetPendingAsync(cancellationToken);
        foreach (var notification in pending)
        {
            var user = notification.User
                ?? throw new InvalidOperationException("Пользователь уведомления не загружен.");
            try
            {
                await notificationSender.SendAsync(user, notification, cancellationToken);
                SubscriptionRules.MarkSent(notification, clock.UtcNow);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Ошибка одного получателя не должна останавливать всю пакетную обработку.
                SubscriptionRules.MarkFailed(notification, exception.Message);
            }
        }

        if (pending.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }
}
