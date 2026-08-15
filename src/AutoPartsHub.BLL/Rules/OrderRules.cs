using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Формирует заказ и контролирует его разрешённые переходы состояния.
/// </summary>
internal static class OrderRules
{
    /// <summary>Создаёт заказ, резервирует товары и сохраняет снимки строк.</summary>
    internal static Order Create(
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

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = ValidationRules.Required(orderNumber, nameof(orderNumber), 40),
            ContactName = ValidationRules.Required(contactName, nameof(contactName), 120),
            Phone = ValidationRules.Required(phone, nameof(phone), 30),
            DeliveryAddress = ValidationRules.Required(
                deliveryAddress,
                nameof(deliveryAddress),
                500),
            DeliveryMethod = deliveryMethod,
            PaymentMethod = paymentMethod,
            // Онлайн-платёж требует подтверждения; оплата при получении сразу
            // передаёт заказ на обработку.
            Status = paymentMethod == PaymentMethod.CardOnline
                ? OrderStatus.PendingPayment
                : OrderStatus.Processing,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var (product, quantity) in lines)
        {
            ProductRules.Reserve(product, quantity);
            // Артикул, название и цена копируются намеренно: изменение каталога
            // не должно переписать уже оформленную покупку.
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Product = product,
                Article = product.Article,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);
        return order;
    }

    /// <summary>Переводит заказ только в разрешённое следующее состояние.</summary>
    internal static void ChangeStatus(Order order, OrderStatus status, DateTimeOffset now)
    {
        if (!CanMove(order.Status, status))
        {
            throw new DomainException(
                $"Переход заказа из {order.Status} в {status} запрещён.");
        }

        order.Status = status;
        order.UpdatedAt = now;
    }

    /// <summary>Проверяет конечный автомат статусов заказа.</summary>
    private static bool CanMove(OrderStatus current, OrderStatus next) => current switch
    {
        OrderStatus.PendingPayment => next is OrderStatus.Paid or OrderStatus.Cancelled,
        OrderStatus.Paid => next is OrderStatus.Processing or OrderStatus.Cancelled,
        OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled,
        OrderStatus.Shipped => next is OrderStatus.Delivered,
        // Delivered и Cancelled — конечные состояния.
        _ => false
    };
}
