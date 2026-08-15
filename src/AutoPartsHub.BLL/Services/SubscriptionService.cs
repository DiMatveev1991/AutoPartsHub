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
/// <param name="repository">Хранилище данных приложения.</param>
/// <param name="notificationSender">Канал отправки уведомлений.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class SubscriptionService(
    IAutoPartsRepository repository,
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
        if (await repository.FindProductAsync(request.ProductId, cancellationToken) is null)
            throw new NotFoundException("Товар не найден.");
        if (await repository.ActiveSubscriptionExistsAsync(
                userId,
                request.ProductId,
                request.Type,
                cancellationToken))
            throw new ConflictException("Такая подписка уже существует.");

        await repository.AddSubscriptionAsync(
            SubscriptionRules.Create(
                userId,
                request.ProductId,
                request.Type,
                request.TargetPrice,
                clock.UtcNow),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает историю уведомлений пользователя.
    /// </summary>
    public async Task<IReadOnlyCollection<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var items = await repository.GetNotificationsAsync(userId, cancellationToken);
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
        var subscriptions = await repository.GetTriggeredSubscriptionsAsync(cancellationToken);
        foreach (var subscription in subscriptions)
        {
            var product = subscription.Product
                ?? throw new InvalidOperationException("Товар подписки не загружен.");
            var text = subscription.Type == SubscriptionType.BackInStock
                ? $"Товар «{product.Name}» ({product.Article}) снова в наличии."
                : $"Цена товара «{product.Name}» снизилась до {product.Price:F2}.";

            await repository.AddNotificationAsync(
                SubscriptionRules.CreateNotification(
                    subscription.UserId,
                    subscription.Type.ToString(),
                    text,
                    clock.UtcNow),
                cancellationToken);
            SubscriptionRules.Complete(subscription);
        }

        // Подписка завершается и уведомление добавляется одним сохранением.
        if (subscriptions.Count > 0)
            await repository.SaveChangesAsync(cancellationToken);
        return subscriptions.Count;
    }

    /// <summary>
    /// Отправляет ожидающие уведомления и фиксирует результат каждой попытки.
    /// </summary>
    public async Task<int> SendPendingAsync(CancellationToken cancellationToken)
    {
        var notifications = await repository.GetPendingNotificationsAsync(cancellationToken);
        foreach (var notification in notifications)
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

        if (notifications.Count > 0)
            await repository.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }
}
