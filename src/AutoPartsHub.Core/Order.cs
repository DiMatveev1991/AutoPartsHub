namespace AutoPartsHub.Core;

/// <summary>
/// Представляет заказ пользователя, его состав и жизненный цикл.
/// </summary>
/// <remarks>
/// Много заказов относится к одному пользователю; один заказ содержит много
/// <see cref="OrderItem"/>.
/// </remarks>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// Создаёт экземпляр заказа для восстановления Entity Framework Core.
    /// </summary>
    private Order()
    {
    }

    /// <summary>
    /// Инициализирует заказ проверенными контактными данными.
    /// </summary>
    private Order(
        Guid userId,
        string orderNumber,
        string contactName,
        string phone,
        string deliveryAddress,
        DeliveryMethod deliveryMethod,
        PaymentMethod paymentMethod,
        DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrderNumber = orderNumber;
        ContactName = Required(contactName, nameof(contactName), 120);
        Phone = Required(phone, nameof(phone), 30);
        DeliveryAddress = Required(deliveryAddress, nameof(deliveryAddress), 500);
        DeliveryMethod = deliveryMethod;
        PaymentMethod = paymentMethod;
        // Онлайн-платёж требует отдельного подтверждения, а оплата при получении
        // позволяет сразу передать заказ в обработку.
        Status = paymentMethod == PaymentMethod.CardOnline
            ? OrderStatus.PendingPayment
            : OrderStatus.Processing;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>Получает уникальный идентификатор заказа.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает внешний ключ пользователя; много заказов относится к одному пользователю.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает человекочитаемый номер заказа.</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>Получает текущий статус заказа.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Получает имя получателя заказа.</summary>
    public string ContactName { get; private set; } = string.Empty;

    /// <summary>Получает контактный телефон получателя.</summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>Получает адрес доставки.</summary>
    public string DeliveryAddress { get; private set; } = string.Empty;

    /// <summary>Получает выбранный способ доставки.</summary>
    public DeliveryMethod DeliveryMethod { get; private set; }

    /// <summary>Получает выбранный способ оплаты.</summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>Получает итоговую стоимость заказа.</summary>
    public decimal Total { get; private set; }

    /// <summary>Получает дату и время создания заказа.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Получает дату и время последнего изменения заказа.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с пользователем.</summary>
    public User? User { get; private set; }

    /// <summary>Получает сторону «многие» связи один-ко-многим с позициями заказа.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items;

    /// <summary>
    /// Создаёт заказ из позиций корзины и резервирует товарные остатки.
    /// </summary>
    public static Order Create(
        Guid userId,
        string orderNumber,
        string contactName,
        string phone,
        string deliveryAddress,
        DeliveryMethod deliveryMethod,
        PaymentMethod paymentMethod,
        IReadOnlyCollection<(Product Product, int Quantity)> lines,
        DateTimeOffset now)
    {
        if (lines.Count == 0)
            throw new DomainException("Нельзя оформить пустую корзину.");

        var order = new Order(
            userId,
            Required(orderNumber, nameof(orderNumber), 40),
            contactName,
            phone,
            deliveryAddress,
            deliveryMethod,
            paymentMethod,
            now);

        foreach (var (product, quantity) in lines)
        {
            // В заказ копируются название и цена: последующие изменения каталога
            // не должны менять уже оформленный заказ.
            product.Reserve(quantity);
            order._items.Add(new OrderItem(
                order.Id,
                product.Id,
                product.Article,
                product.Name,
                product.Price,
                quantity));
        }

        order.Total = order._items.Sum(item => item.UnitPrice * item.Quantity);
        return order;
    }

    /// <summary>
    /// Переводит заказ в следующий разрешённый статус.
    /// </summary>
    public void ChangeStatus(OrderStatus status, DateTimeOffset now)
    {
        if (!CanMove(Status, status))
            throw new DomainException($"Переход заказа из {Status} в {status} запрещён.");

        Status = status;
        UpdatedAt = now;
    }

    /// <summary>
    /// Проверяет допустимость перехода между статусами заказа.
    /// </summary>
    private static bool CanMove(OrderStatus current, OrderStatus next) =>
        // Delivered и Cancelled намеренно отсутствуют: это конечные состояния,
        // из которых дальнейшие переходы запрещены.
        current switch
        {
            OrderStatus.PendingPayment => next is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next is OrderStatus.Delivered,
            _ => false
        };

    /// <summary>
    /// Проверяет обязательную строку, удаляет крайние пробелы и контролирует длину.
    /// </summary>
    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}
