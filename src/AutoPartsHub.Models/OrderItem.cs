using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит снимок товарной позиции на момент оформления заказа.
/// </summary>
public class OrderItem : Entity
{
    /// <summary>Получает или задаёт внешний ключ заказа.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Получает или задаёт внешний ключ исходного товара.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Получает или задаёт снимок артикула.</summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>Получает или задаёт снимок названия товара.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Получает или задаёт цену единицы на момент оформления.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Получает или задаёт заказанное количество.</summary>
    public int Quantity { get; set; }

    /// <summary>Получает или задаёт заказ по связи многие-к-одному.</summary>
    public Order? Order { get; set; }

    /// <summary>Получает или задаёт исходный товар по связи многие-к-одному.</summary>
    public Product? Product { get; set; }
}
