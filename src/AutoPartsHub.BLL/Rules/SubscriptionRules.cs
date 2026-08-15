using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Управляет подписками и состоянием создаваемых по ним уведомлений.
/// </summary>
internal static class SubscriptionRules
{
    /// <summary>Создаёт активную подписку и проверяет целевую цену.</summary>
    internal static ProductSubscription Create(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        decimal? targetPrice,
        DateTimeOffset createdAt)
    {
        if (type == SubscriptionType.PriceDrop && (targetPrice is null || targetPrice <= 0))
            throw new DomainException("Для подписки на снижение цены нужна целевая цена.");

        return new ProductSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            Type = type,
            TargetPrice = targetPrice,
            IsActive = true,
            CreatedAt = createdAt
        };
    }

    /// <summary>Проверяет выполнение условия активной подписки.</summary>
    internal static bool IsTriggered(ProductSubscription subscription, Product product) =>
        subscription.IsActive && product.IsActive &&
        (subscription.Type == SubscriptionType.BackInStock && product.Stock > 0 ||
         subscription.Type == SubscriptionType.PriceDrop && product.Price <= subscription.TargetPrice);

    /// <summary>Завершает одноразовую подписку после создания уведомления.</summary>
    internal static void Complete(ProductSubscription subscription) => subscription.IsActive = false;

    /// <summary>Создаёт уведомление в состоянии ожидания отправки.</summary>
    internal static Notification CreateNotification(
        Guid userId,
        string type,
        string text,
        DateTimeOffset createdAt) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ValidationRules.Required(type, nameof(type), 60),
            Text = ValidationRules.Required(text, nameof(text), 1000),
            Status = NotificationStatus.Pending,
            CreatedAt = createdAt
        };

    /// <summary>Отмечает уведомление успешно отправленным.</summary>
    internal static void MarkSent(Notification notification, DateTimeOffset sentAt)
    {
        notification.Status = NotificationStatus.Sent;
        notification.SentAt = sentAt;
        notification.Error = null;
    }

    /// <summary>Фиксирует ограниченное описание неуспешной отправки.</summary>
    internal static void MarkFailed(Notification notification, string error)
    {
        // Failed не выбирается повторно worker-ом: постоянная ошибка Telegram
        // не превращается в бесконечную очередь, а причина остаётся для диагностики.
        notification.Status = NotificationStatus.Failed;
        notification.Error = string.IsNullOrWhiteSpace(error)
            ? "Unknown error"
            : error[..Math.Min(error.Length, 1000)];
    }
}
