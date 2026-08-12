namespace AutoPartsHub.Core;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

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
        Status = paymentMethod == PaymentMethod.CardOnline
            ? OrderStatus.PendingPayment
            : OrderStatus.Processing;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string DeliveryAddress { get; private set; } = string.Empty;
    public DeliveryMethod DeliveryMethod { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public User? User { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

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

    public void ChangeStatus(OrderStatus status, DateTimeOffset now)
    {
        if (!CanMove(Status, status))
            throw new DomainException($"Переход заказа из {Status} в {status} запрещён.");

        Status = status;
        UpdatedAt = now;
    }

    private static bool CanMove(OrderStatus current, OrderStatus next) =>
        current switch
        {
            OrderStatus.PendingPayment => next is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next is OrderStatus.Delivered,
            _ => false
        };

    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}
