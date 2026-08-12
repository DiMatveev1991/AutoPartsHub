namespace AutoPartsHub.Core;

public sealed class ProductSubscription
{
    private ProductSubscription()
    {
    }

    public ProductSubscription(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        decimal? targetPrice,
        DateTimeOffset createdAt)
    {
        if (type == SubscriptionType.PriceDrop && (targetPrice is null || targetPrice <= 0))
            throw new DomainException("Для подписки на снижение цены нужна целевая цена.");

        Id = Guid.NewGuid();
        UserId = userId;
        ProductId = productId;
        Type = type;
        TargetPrice = targetPrice;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public SubscriptionType Type { get; private set; }
    public decimal? TargetPrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public User? User { get; private set; }
    public Product? Product { get; private set; }

    public bool IsTriggeredBy(Product product) =>
        IsActive && product.IsActive &&
        (Type == SubscriptionType.BackInStock && product.Stock > 0 ||
         Type == SubscriptionType.PriceDrop && product.Price <= TargetPrice);

    public void Complete() => IsActive = false;
}
