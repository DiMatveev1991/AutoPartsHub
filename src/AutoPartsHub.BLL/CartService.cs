using AutoPartsHub.DTOs;
using AutoPartsHub.Models;

namespace AutoPartsHub.BLL;

/// <summary>
/// Выполняет операции над пользовательской корзиной.
/// </summary>
/// <param name="repository">Хранилище данных приложения.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class CartService(IAutoPartsRepository repository, IClock clock)
{
    /// <summary>
    /// Возвращает корзину пользователя, создавая её при первом обращении.
    /// </summary>
    public async Task<CartDto> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        // Корзина создаётся лениво при первом обращении. Сохранение даёт ей
        // стабильный Id и позволяет уникальному индексу гарантировать одну корзину на пользователя.
        await repository.SaveChangesAsync(cancellationToken);
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
        var product = await repository.FindProductAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        cart.Add(product, request.Quantity, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
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
        var cart = await repository.FindCartAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Корзина не найдена.");
        var product = await repository.FindProductAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");

        cart.ChangeQuantity(productId, request.Quantity, product.Stock, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
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
        var cart = await repository.FindCartAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Корзина не найдена.");
        cart.Remove(productId, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }

    /// <summary>
    /// Загружает существующую корзину или создаёт новую доменную сущность.
    /// </summary>
    private async Task<Cart> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await repository.FindCartAsync(userId, cancellationToken);
        if (cart is not null)
            return cart;

        cart = new Cart(userId, clock.UtcNow);
        // AddCartAsync только добавляет сущность в Change Tracker. Транзакционная
        // граница и момент SaveChanges остаются у публичного сценария-вызывателя.
        await repository.AddCartAsync(cart, cancellationToken);
        return cart;
    }
}
