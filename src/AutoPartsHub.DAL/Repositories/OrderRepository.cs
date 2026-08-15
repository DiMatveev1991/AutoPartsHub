using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение заказов через EF Core.</summary>
internal sealed class OrderRepository(AutoPartsDbContext db) : IOrderRepository
{
    /// <summary>
    /// Возвращает историю всех заказов или заказов конкретного пользователя вместе с позициями.
    /// Результат не отслеживается, поскольку список используется для просмотра и не изменяется этим сценарием.
    /// </summary>
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

    /// <summary>
    /// Загружает заказ с позициями как отслеживаемый граф для последующего изменения статуса.
    /// Отслеживание позволяет сохранить переход состояния через общий Unit of Work без отдельного update-метода.
    /// </summary>
    public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Orders
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>
    /// Регистрирует новый заказ и его дочерние позиции в контексте EF Core.
    /// Фактическая запись выполняется Unit of Work внутри транзакции оформления заказа.
    /// </summary>
    public async Task AddAsync(Order order, CancellationToken cancellationToken) =>
        await db.Orders.AddAsync(order, cancellationToken);
}
