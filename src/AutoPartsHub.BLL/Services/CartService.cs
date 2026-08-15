using AutoPartsHub.BLL;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.BLL.Services;

/// <summary>
/// Выполняет операции над пользовательской корзиной.
/// </summary>
/// <param name="carts">Хранилище корзин.</param>
/// <param name="catalog">Хранилище каталога.</param>
/// <param name="unitOfWork">Граница сохранения изменений.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class CartService(
    ICartRepository carts,
    ICatalogRepository catalog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICartService
{
    /// <summary>
    /// Возвращает корзину пользователя, создавая её при первом обращении.
    /// </summary>
    public async Task<CartDto> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        // Корзина создаётся лениво при первом обращении. Сохранение даёт ей
        // стабильный Id и позволяет уникальному индексу гарантировать одну корзину на пользователя.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }

    /// <summary>
    /// Добавляет товар в корзину пользователя.
    /// </summary>
    public async Task<CartDto> AddAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var product = await catalog.FindProductAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        CartRules.Add(cart, product, request.Quantity, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }

    /// <summary>
    /// Изменяет количество выбранного товара в корзине.
    /// </summary>
    public async Task<CartDto> ChangeQuantityAsync(
        Guid userId,
        Guid productId,
        ChangeCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await carts.FindByUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Корзина не найдена.");
        var product = await catalog.FindProductAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");

        CartRules.ChangeQuantity(cart, productId, request.Quantity, product.Stock, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }

    /// <summary>
    /// Удаляет товар из корзины пользователя.
    /// </summary>
    public async Task<CartDto> RemoveAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var cart = await carts.FindByUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Корзина не найдена.");
        CartRules.Remove(cart, productId, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }

    /// <summary>
    /// Загружает существующую корзину или создаёт новую доменную сущность.
    /// </summary>
    private async Task<Cart> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await carts.FindByUserAsync(userId, cancellationToken);
        if (cart is not null)
            return cart;

        cart = CartRules.Create(userId, clock.UtcNow);
        // AddCartAsync только добавляет сущность в Change Tracker. Транзакционная
        // граница и момент SaveChanges остаются у публичного сценария-вызывателя.
        await carts.AddAsync(cart, cancellationToken);
        return cart;
    }
}
