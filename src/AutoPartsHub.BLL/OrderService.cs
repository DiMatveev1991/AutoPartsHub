using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

public sealed class OrderService(
    IAutoPartsRepository repository,
    IOrderNumberGenerator orderNumbers,
    IClock clock)
{
    public Task<OrderDto> CheckoutAsync(
        Guid userId,
        CheckoutRequest request,
        CancellationToken cancellationToken) =>
        repository.ExecuteInTransactionAsync(async transactionToken =>
        {
            var cart = await repository.FindCartAsync(userId, transactionToken);
            if (cart is null || cart.Items.Count == 0)
                throw new DomainException("Нельзя оформить пустую корзину.");

            var lines = cart.Items.Select(item =>
            {
                var product = item.Product
                    ?? throw new InvalidOperationException("Товар корзины не загружен.");
                return (product, item.Quantity);
            }).ToArray();

            var now = clock.UtcNow;
            var order = Order.Create(
                userId,
                orderNumbers.Next(now),
                request.ContactName,
                request.Phone,
                request.DeliveryAddress,
                request.DeliveryMethod,
                request.PaymentMethod,
                lines,
                now);

            await repository.AddOrderAsync(order, transactionToken);
            cart.Clear(now);
            await repository.SaveChangesAsync(transactionToken);
            return order.ToDto();
        }, cancellationToken);

    public async Task<IReadOnlyCollection<OrderDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(userId, cancellationToken);
        return orders.Select(order => order.ToDto()).ToArray();
    }

    public async Task<OrderDto> GetMineAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await repository.FindOrderAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
            throw new NotFoundException("Заказ не найден.");
        return order.ToDto();
    }

    public async Task<OrderDto> FindByNumberAsync(
        Guid userId,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(userId, cancellationToken);
        var order = orders.SingleOrDefault(item =>
            string.Equals(item.OrderNumber, orderNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("Заказ не найден.");
        return order.ToDto();
    }
}
