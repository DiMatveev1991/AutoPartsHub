using AutoPartsHub.Models;

namespace AutoPartsHub.DTOs;

/// <summary>
/// Содержит данные для добавления товара в корзину.
/// </summary>
/// <param name="ProductId">Идентификатор товара.</param>
/// <param name="Quantity">Добавляемое количество.</param>
public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

/// <summary>
/// Содержит новое количество товарной позиции корзины.
/// </summary>
/// <param name="Quantity">Новое количество.</param>
public sealed record ChangeCartItemRequest(int Quantity);

/// <summary>
/// Представляет одну позицию корзины.
/// </summary>
/// <param name="ProductId">Идентификатор товара.</param>
/// <param name="Article">Артикул товара.</param>
/// <param name="Name">Название товара.</param>
/// <param name="UnitPrice">Цена единицы.</param>
/// <param name="Quantity">Количество в корзине.</param>
/// <param name="AvailableStock">Доступный остаток.</param>
/// <param name="LineTotal">Стоимость позиции.</param>
public sealed record CartItemDto(
    Guid ProductId,
    string Article,
    string Name,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock,
    decimal LineTotal);

/// <summary>
/// Представляет корзину пользователя.
/// </summary>
/// <param name="Id">Идентификатор корзины.</param>
/// <param name="Items">Позиции корзины.</param>
/// <param name="Total">Общая стоимость.</param>
public sealed record CartDto(
    Guid Id,
    IReadOnlyCollection<CartItemDto> Items,
    decimal Total);

/// <summary>
/// Содержит данные для оформления заказа.
/// </summary>
/// <param name="ContactName">Имя получателя.</param>
/// <param name="Phone">Контактный телефон.</param>
/// <param name="DeliveryAddress">Адрес доставки.</param>
/// <param name="DeliveryMethod">Способ доставки.</param>
/// <param name="PaymentMethod">Способ оплаты.</param>
public sealed record CheckoutRequest(
    string ContactName,
    string Phone,
    string DeliveryAddress,
    DeliveryMethod DeliveryMethod,
    PaymentMethod PaymentMethod);

/// <summary>
/// Представляет одну позицию оформленного заказа.
/// </summary>
/// <param name="ProductId">Идентификатор исходного товара.</param>
/// <param name="Article">Снимок артикула.</param>
/// <param name="ProductName">Снимок названия.</param>
/// <param name="UnitPrice">Цена единицы на момент оформления.</param>
/// <param name="Quantity">Заказанное количество.</param>
/// <param name="LineTotal">Стоимость позиции.</param>
public sealed record OrderItemDto(
    Guid ProductId,
    string Article,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

/// <summary>
/// Представляет заказ пользователя.
/// </summary>
/// <param name="Id">Идентификатор заказа.</param>
/// <param name="OrderNumber">Номер заказа.</param>
/// <param name="Status">Текущий статус.</param>
/// <param name="ContactName">Имя получателя.</param>
/// <param name="Phone">Контактный телефон.</param>
/// <param name="DeliveryAddress">Адрес доставки.</param>
/// <param name="DeliveryMethod">Способ доставки.</param>
/// <param name="PaymentMethod">Способ оплаты.</param>
/// <param name="Total">Общая стоимость.</param>
/// <param name="CreatedAt">Дата и время оформления.</param>
/// <param name="Items">Позиции заказа.</param>
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

/// <summary>
/// Содержит параметры товарной подписки.
/// </summary>
/// <param name="ProductId">Идентификатор товара.</param>
/// <param name="Type">Тип подписки.</param>
/// <param name="TargetPrice">Целевая цена для подписки на снижение стоимости.</param>
public sealed record SubscribeRequest(
    Guid ProductId,
    SubscriptionType Type,
    decimal? TargetPrice);

/// <summary>
/// Представляет уведомление пользователя.
/// </summary>
/// <param name="Id">Идентификатор уведомления.</param>
/// <param name="Type">Тип уведомления.</param>
/// <param name="Text">Текст уведомления.</param>
/// <param name="Status">Статус отправки.</param>
/// <param name="CreatedAt">Дата и время создания.</param>
/// <param name="SentAt">Дата и время успешной отправки.</param>
public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Text,
    NotificationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt);
