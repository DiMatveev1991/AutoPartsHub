using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения заказов.</summary>
public interface IOrderRepository
{
    /// <summary>Возвращает заказы всех пользователей или одного пользователя.</summary>
    Task<IReadOnlyCollection<Order>> GetAsync(
        Guid? userId,
        CancellationToken cancellationToken);

    /// <summary>Находит заказ по идентификатору.</summary>
    Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Добавляет заказ в контекст хранения.</summary>
    Task AddAsync(Order order, CancellationToken cancellationToken);
}
