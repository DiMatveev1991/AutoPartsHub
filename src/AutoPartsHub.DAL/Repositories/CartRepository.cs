using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение корзины через EF Core.</summary>
internal sealed class CartRepository(AutoPartsDbContext db) : ICartRepository
{
    /// <summary>
    /// Загружает корзину пользователя вместе с позициями и товарами как отслеживаемый граф.
    /// Отслеживание необходимо: BLL изменяет количество позиций, а Unit of Work сохраняет весь граф одной операцией.
    /// </summary>
    public Task<Cart?> FindByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Carts
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    /// <summary>
    /// Регистрирует новую корзину в EF Core без немедленной записи в базу данных.
    /// Отложенное сохранение позволяет BLL объединить создание корзины и её позиций в одну единицу работы.
    /// </summary>
    public async Task AddAsync(Cart cart, CancellationToken cancellationToken) =>
        await db.Carts.AddAsync(cart, cancellationToken);
}
