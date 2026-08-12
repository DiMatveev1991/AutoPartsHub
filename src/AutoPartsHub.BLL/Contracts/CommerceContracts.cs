using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

public sealed record ChangeCartItemRequest(int Quantity);

public sealed record CartItemDto(
    Guid ProductId,
    string Article,
    string Name,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock,
    decimal LineTotal);

public sealed record CartDto(
    Guid Id,
    IReadOnlyCollection<CartItemDto> Items,
    decimal Total);

public sealed record CheckoutRequest(
    string ContactName,
    string Phone,
    string DeliveryAddress,
    DeliveryMethod DeliveryMethod,
    PaymentMethod PaymentMethod);

public sealed record OrderItemDto(
    Guid ProductId,
    string Article,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    string ContactName,
    string Phone,
    string DeliveryAddress,
    DeliveryMethod DeliveryMethod,
    PaymentMethod PaymentMethod,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record SubscribeRequest(
    Guid ProductId,
    SubscriptionType Type,
    decimal? TargetPrice);

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Text,
    NotificationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt);
