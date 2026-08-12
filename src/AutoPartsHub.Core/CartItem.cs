namespace AutoPartsHub.Core;

public sealed class CartItem
{
    private CartItem()
    {
    }

    internal CartItem(Guid cartId, Guid productId, int quantity)
    {
        CartId = cartId;
        ProductId = productId;
        ChangeQuantity(quantity);
    }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Cart? Cart { get; private set; }
    public Product? Product { get; private set; }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        Quantity = quantity;
    }
}
