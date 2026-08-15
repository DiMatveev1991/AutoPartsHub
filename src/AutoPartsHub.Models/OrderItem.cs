namespace AutoPartsHub.Models;

/// <summary>
/// Представляет неизменяемый снимок товарной позиции в заказе.
/// </summary>
/// <remarks>
/// Каждая позиция относится к одному заказу и одному исходному товару; обе
/// связи имеют тип многие-к-одному.
/// </remarks>
public sealed class OrderItem
{
    /// <summary>
    /// Создаёт экземпляр позиции заказа для восстановления Entity Framework Core.
    /// </summary>
    private OrderItem()
    {
    }

    /// <summary>
    /// Создаёт снимок товара с ценой и названием на момент оформления заказа.
    /// </summary>
    internal OrderItem(
        Guid orderId,
        Guid productId,
        string article,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Article = article;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    /// <summary>Получает уникальный идентификатор позиции заказа.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает внешний ключ заказа; много позиций относится к одному заказу.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Получает внешний ключ товара; много позиций заказа относится к одному товару.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает снимок артикула товара.</summary>
    public string Article { get; private set; } = string.Empty;

    /// <summary>Получает снимок названия товара.</summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>Получает цену единицы товара на момент оформления.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Получает заказанное количество товара.</summary>
    public int Quantity { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с заказом.</summary>
    public Order? Order { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с исходным товаром.</summary>
    public Product? Product { get; private set; }
}
