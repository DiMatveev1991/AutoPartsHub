namespace AutoPartsHub.Core;

/// <summary>
/// Представляет одну товарную позицию корзины.
/// </summary>
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

    /// <summary>Получает идентификатор корзины.</summary>
    public Guid CartId { get; private set; }

    /// <summary>Получает идентификатор товара.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает количество товара.</summary>
    public int Quantity { get; private set; }

    /// <summary>Получает связанную корзину при загрузке из базы данных.</summary>
    public Cart? Cart { get; private set; }

    /// <summary>Получает связанный товар при загрузке из базы данных.</summary>
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
