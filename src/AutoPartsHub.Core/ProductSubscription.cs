namespace AutoPartsHub.Core;

/// <summary>
/// Представляет подписку пользователя на наличие товара или снижение цены.
/// </summary>
public sealed class ProductSubscription
{
    /// <summary>
    /// Создаёт экземпляр подписки для восстановления Entity Framework Core.
    /// </summary>
    private ProductSubscription()
    {
    }

    /// <summary>
    /// Создаёт активную подписку и проверяет параметры выбранного типа.
    /// </summary>
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

    /// <summary>Получает уникальный идентификатор подписки.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает идентификатор подписанного пользователя.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает идентификатор отслеживаемого товара.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает тип подписки.</summary>
    public SubscriptionType Type { get; private set; }

    /// <summary>Получает целевую цену для подписки на снижение стоимости.</summary>
    public decimal? TargetPrice { get; private set; }

    /// <summary>Указывает, ожидает ли подписка срабатывания.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Получает дату и время создания подписки.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Получает подписанного пользователя при загрузке связи из базы данных.</summary>
    public User? User { get; private set; }

    /// <summary>Получает отслеживаемый товар при загрузке связи из базы данных.</summary>
    public Product? Product { get; private set; }

    /// <summary>
    /// Определяет, выполнено ли условие подписки текущим состоянием товара.
    /// </summary>
    public bool IsTriggeredBy(Product product) =>
        IsActive && product.IsActive &&
        (Type == SubscriptionType.BackInStock && product.Stock > 0 ||
         Type == SubscriptionType.PriceDrop && product.Price <= TargetPrice);

    /// <summary>
    /// Завершает подписку после создания уведомления.
    /// </summary>
    public void Complete() => IsActive = false;
}
