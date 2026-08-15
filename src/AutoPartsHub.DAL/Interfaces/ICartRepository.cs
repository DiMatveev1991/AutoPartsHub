using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения корзины.</summary>
public interface ICartRepository
{
    /// <summary>Находит корзину пользователя вместе с товарными позициями.</summary>
    Task<Cart?> FindByUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Добавляет корзину в контекст хранения.</summary>
    Task AddAsync(Cart cart, CancellationToken cancellationToken);
}
