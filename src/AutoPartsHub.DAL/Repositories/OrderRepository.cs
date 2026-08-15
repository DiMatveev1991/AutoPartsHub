using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение заказов через EF Core.</summary>
internal sealed class OrderRepository(AutoPartsDbContext db) : IOrderRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Order>> GetAsync(
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = db.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .AsSplitQuery()
            .AsQueryable();
        if (userId is not null)
            query = query.Where(item => item.UserId == userId);

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Orders
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Order order, CancellationToken cancellationToken) =>
        await db.Orders.AddAsync(order, cancellationToken);
}
