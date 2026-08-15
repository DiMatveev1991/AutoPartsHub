using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения товарных подписок.</summary>
public interface ISubscriptionRepository
{
    /// <summary>Проверяет наличие активной подписки заданного типа.</summary>
    Task<bool> ActiveExistsAsync(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        CancellationToken cancellationToken);

    /// <summary>Добавляет товарную подписку в контекст хранения.</summary>
    Task AddAsync(ProductSubscription subscription, CancellationToken cancellationToken);

    /// <summary>Возвращает активные подписки, условия которых уже выполнены.</summary>
    Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredAsync(
        CancellationToken cancellationToken);
}
