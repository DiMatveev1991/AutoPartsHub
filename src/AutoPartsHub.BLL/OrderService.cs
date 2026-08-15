using AutoPartsHub.DTOs;
using AutoPartsHub.Models;

namespace AutoPartsHub.BLL;

/// <summary>
/// Оформляет заказы и предоставляет пользователю историю покупок.
/// </summary>
/// <param name="repository">Хранилище данных приложения.</param>
/// <param name="orderNumbers">Генератор номеров заказов.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class OrderService(
    IAutoPartsRepository repository,
    IOrderNumberGenerator orderNumbers,
    IClock clock)
{
    /// <summary>
    /// Оформляет содержимое корзины как заказ в одной транзакции.
    /// </summary>
    public Task<OrderDto> CheckoutAsync(
        Guid userId,
        CheckoutRequest request,
        CancellationToken cancellationToken) =>
        repository.ExecuteInTransactionAsync(async transactionToken =>
        {
            // Загрузка, резервирование остатков, создание заказа и очистка корзины
            // должны завершиться целиком либо быть полностью отменены.
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

    /// <summary>
    /// Возвращает историю заказов пользователя.
    /// </summary>
    public async Task<IReadOnlyCollection<OrderDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(userId, cancellationToken);
        return orders.Select(order => order.ToDto()).ToArray();
    }

    /// <summary>
    /// Возвращает заказ пользователя по внутреннему идентификатору.
    /// </summary>
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

    /// <summary>
    /// Находит заказ пользователя по человекочитаемому номеру.
    /// </summary>
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
