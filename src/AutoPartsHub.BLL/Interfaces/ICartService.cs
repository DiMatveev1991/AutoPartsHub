using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает операции с пользовательской корзиной.
/// </summary>
public interface ICartService
{
    /// <summary>Возвращает корзину пользователя.</summary>
    Task<CartDto> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Добавляет товар в корзину.</summary>
    Task<CartDto> AddAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken);

    /// <summary>Изменяет количество товарной позиции.</summary>
    Task<CartDto> ChangeQuantityAsync(
        Guid userId,
        Guid productId,
        ChangeCartItemRequest request,
        CancellationToken cancellationToken);

    /// <summary>Удаляет товар из корзины.</summary>
    Task<CartDto> RemoveAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken);
}
