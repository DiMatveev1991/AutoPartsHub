namespace AutoPartsHub.Core;

/// <summary>
/// Представляет одну товарную позицию корзины.
/// </summary>
/// <remarks>
/// Каждая позиция относится к одной корзине и одному товару; обе связи имеют
/// тип многие-к-одному. Пара <see cref="CartId"/> и <see cref="ProductId"/>
/// образует составной первичный ключ.
/// </remarks>
public sealed class CartItem
{
    /// <summary>
    /// Создаёт экземпляр позиции корзины для восстановления Entity Framework Core.
    /// </summary>
    private CartItem()
    {
    }

    /// <summary>
    /// Создаёт позицию корзины с указанным количеством товара.
    /// </summary>
    internal CartItem(Guid cartId, Guid productId, int quantity)
    {
        CartId = cartId;
        ProductId = productId;
        ChangeQuantity(quantity);
    }

    /// <summary>Получает внешний ключ корзины и первую часть составного первичного ключа.</summary>
    public Guid CartId { get; private set; }

    /// <summary>Получает внешний ключ товара и вторую часть составного первичного ключа.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает количество товара.</summary>
    public int Quantity { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с корзиной.</summary>
    public Cart? Cart { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с товаром.</summary>
    public Product? Product { get; private set; }

    /// <summary>
    /// Устанавливает положительное количество товара.
    /// </summary>
    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        Quantity = quantity;
    }
}
