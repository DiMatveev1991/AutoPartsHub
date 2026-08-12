namespace AutoPartsHub.Core;

/// <summary>
/// Представляет неизменяемый снимок товарной позиции в заказе.
/// </summary>
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

    /// <summary>Получает идентификатор заказа.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Получает идентификатор исходного товара.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает снимок артикула товара.</summary>
    public string Article { get; private set; } = string.Empty;

    /// <summary>Получает снимок названия товара.</summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>Получает цену единицы товара на момент оформления.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Получает заказанное количество товара.</summary>
    public int Quantity { get; private set; }

    /// <summary>Получает связанный заказ при загрузке из базы данных.</summary>
    public Order? Order { get; private set; }

    /// <summary>Получает исходный товар при загрузке из базы данных.</summary>
    public Product? Product { get; private set; }
}
