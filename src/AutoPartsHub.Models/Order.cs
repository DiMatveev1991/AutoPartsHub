using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит заказ пользователя и его текущее состояние.
/// </summary>
/// <remarks>
/// Создание заказа, резервирование остатков и переходы статусов находятся в BLL.
/// </remarks>
public class Order : Entity
{
    /// <summary>Получает или задаёт внешний ключ пользователя.</summary>
    public Guid UserId { get; set; }

    /// <summary>Получает или задаёт человекочитаемый номер заказа.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Получает или задаёт текущий статус заказа.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Получает или задаёт имя получателя.</summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>Получает или задаёт контактный телефон.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Получает или задаёт адрес доставки.</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Получает или задаёт способ доставки.</summary>
    public DeliveryMethod DeliveryMethod { get; set; }

    /// <summary>Получает или задаёт способ оплаты.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Получает или задаёт итоговую стоимость.</summary>
    public decimal Total { get; set; }

    /// <summary>Получает или задаёт дату создания.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Получает или задаёт дату последнего изменения.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Получает или задаёт пользователя по связи многие-к-одному.</summary>
    public User? User { get; set; }

    /// <summary>Получает или задаёт позиции заказа.</summary>
    public ICollection<OrderItem> Items { get; set; } = [];
}
