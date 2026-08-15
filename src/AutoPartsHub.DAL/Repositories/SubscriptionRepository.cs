using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение товарных подписок через EF Core.</summary>
internal sealed class SubscriptionRepository(AutoPartsDbContext db) : ISubscriptionRepository
{
    /// <inheritdoc />
    public Task<bool> ActiveExistsAsync(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        CancellationToken cancellationToken) =>
        db.ProductSubscriptions.AnyAsync(
            item => item.UserId == userId &&
                    item.ProductId == productId &&
                    item.Type == type &&
                    item.IsActive,
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(
        ProductSubscription subscription,
        CancellationToken cancellationToken) =>
        await db.ProductSubscriptions.AddAsync(subscription, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredAsync(
        CancellationToken cancellationToken) =>
        // Условие повторяет SubscriptionRules из BLL в форме, переводимой в SQL.
        // DAL не зависит от BLL, а фильтрация не загружает все подписки в память.
        await db.ProductSubscriptions
            .Include(item => item.Product)
            .Where(item => item.IsActive &&
                (item.Type == SubscriptionType.BackInStock && item.Product!.Stock > 0 ||
                 item.Type == SubscriptionType.PriceDrop && item.Product!.Price <= item.TargetPrice))
            .ToArrayAsync(cancellationToken);
}
