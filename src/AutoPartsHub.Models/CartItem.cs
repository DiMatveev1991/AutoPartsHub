namespace AutoPartsHub.Models;

/// <summary>
/// Хранит одну товарную позицию корзины.
/// </summary>
public class CartItem
{
    /// <summary>Получает или задаёт внешний ключ корзины и часть составного ключа.</summary>
    public Guid CartId { get; set; }

    /// <summary>Получает или задаёт внешний ключ товара и часть составного ключа.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Получает или задаёт количество товара.</summary>
    public int Quantity { get; set; }

    /// <summary>Получает или задаёт корзину по связи многие-к-одному.</summary>
    public Cart? Cart { get; set; }

    /// <summary>Получает или задаёт товар по связи многие-к-одному.</summary>
    public Product? Product { get; set; }
}
