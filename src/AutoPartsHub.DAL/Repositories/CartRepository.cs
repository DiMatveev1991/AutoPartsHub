using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение корзины через EF Core.</summary>
internal sealed class CartRepository(AutoPartsDbContext db) : ICartRepository
{
    /// <inheritdoc />
    public Task<Cart?> FindByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Carts
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Cart cart, CancellationToken cancellationToken) =>
        await db.Carts.AddAsync(cart, cancellationToken);
}
