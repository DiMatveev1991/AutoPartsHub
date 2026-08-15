namespace AutoPartsHub.Core;

/// <summary>
/// Представляет подписку пользователя на наличие товара или снижение цены.
/// </summary>
/// <remarks>
/// Каждая подписка относится к одному пользователю и одному товару; со стороны
/// пользователя и товара это связи один-ко-многим.
/// </remarks>
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

    /// <summary>Получает внешний ключ пользователя; один пользователь имеет много подписок.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает внешний ключ товара; один товар может отслеживаться многими подписками.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает тип подписки.</summary>
    public SubscriptionType Type { get; private set; }

    /// <summary>Получает целевую цену для подписки на снижение стоимости.</summary>
    public decimal? TargetPrice { get; private set; }

    /// <summary>Указывает, ожидает ли подписка срабатывания.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Получает дату и время создания подписки.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с пользователем.</summary>
    public User? User { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с товаром.</summary>
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
