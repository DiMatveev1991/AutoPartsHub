namespace AutoPartsHub.Core;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

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

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Article { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Order? Order { get; private set; }
    public Product? Product { get; private set; }
}
