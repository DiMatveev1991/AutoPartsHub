using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

/// <summary>
/// Содержит преобразования доменных сущностей в контракты бизнес-слоя.
/// </summary>
internal static class Mapping
{
    /// <summary>
    /// Преобразует пользователя в DTO.
    /// </summary>
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.TelegramChatId, user.DisplayName, user.Role);

    /// <summary>
    /// Преобразует товар и его совместимость в DTO.
    /// </summary>
    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.Article,
            product.Name,
            product.Description,
            product.Condition,
            product.Price,
            product.Stock,
            product.IsActive,
            product.Compatibilities
                .Select(item => new CompatibilityDto(
                    item.Make,
                    item.Model,
                    item.YearFrom,
                    item.YearTo,
                    item.Engine))
                .ToArray());

    /// <summary>
    /// Преобразует корзину в DTO и рассчитывает стоимость строк и итог.
    /// </summary>
    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items.Select(item =>
        {
            var product = item.Product
                ?? throw new InvalidOperationException("Товар корзины не загружен.");
            return new CartItemDto(
                product.Id,
                product.Article,
                product.Name,
                product.Price,
                item.Quantity,
                product.Stock,
                product.Price * item.Quantity);
        }).ToArray();

        return new CartDto(cart.Id, items, items.Sum(item => item.LineTotal));
    }

    /// <summary>
    /// Преобразует заказ и его позиции в DTO.
    /// </summary>
    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.ContactName,
            order.Phone,
            order.DeliveryAddress,
            order.DeliveryMethod,
            order.PaymentMethod,
            order.Total,
            order.CreatedAt,
            order.Items.Select(item => new OrderItemDto(
                item.ProductId,
                item.Article,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.UnitPrice * item.Quantity)).ToArray());
}
