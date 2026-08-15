using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит корзину пользователя и её позиции.
/// </summary>
/// <remarks>
/// Добавление, объединение и удаление позиций выполняет <c>CartService</c> в BLL.
/// </remarks>
public class Cart : Entity
{
    /// <summary>Получает или задаёт уникальный внешний ключ пользователя.</summary>
    public Guid UserId { get; set; }

    /// <summary>Получает или задаёт дату последнего изменения.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Получает или задаёт пользователя по связи один-к-одному.</summary>
    public User? User { get; set; }

    /// <summary>Получает или задаёт позиции корзины.</summary>
    public ICollection<CartItem> Items { get; set; } = [];
}
