using AutoPartsHub.Models;

namespace AutoPartsHub.DTOs;

/// <summary>
/// Содержит правило совместимости при создании или изменении товара.
/// </summary>
/// <param name="Make">Марка автомобиля.</param>
/// <param name="Model">Модель автомобиля.</param>
/// <param name="YearFrom">Начальный год выпуска.</param>
/// <param name="YearTo">Конечный год выпуска.</param>
/// <param name="Engine">Обозначение двигателя.</param>
public sealed record CompatibilityRequest(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);

/// <summary>
/// Содержит данные для создания товара.
/// </summary>
/// <param name="CategoryId">Идентификатор категории.</param>
/// <param name="Article">Артикул товара.</param>
/// <param name="Name">Название товара.</param>
/// <param name="Description">Описание товара.</param>
/// <param name="Condition">Состояние товара.</param>
/// <param name="Price">Цена товара.</param>
/// <param name="Stock">Начальный остаток.</param>
/// <param name="Compatibilities">Правила совместимости.</param>
public sealed record CreateProductRequest(
    Guid CategoryId,
    string Article,
    string Name,
    string Description,
    ProductCondition Condition,
    decimal Price,
    int Stock,
    IReadOnlyCollection<CompatibilityRequest> Compatibilities);

/// <summary>
/// Содержит изменяемые данные товара.
/// </summary>
/// <param name="CategoryId">Идентификатор категории.</param>
/// <param name="Name">Название товара.</param>
/// <param name="Description">Описание товара.</param>
/// <param name="Condition">Состояние товара.</param>
/// <param name="Price">Цена товара.</param>
/// <param name="Stock">Доступный остаток.</param>
/// <param name="Compatibilities">Правила совместимости.</param>
public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Description,
    ProductCondition Condition,
    decimal Price,
    int Stock,
    IReadOnlyCollection<CompatibilityRequest> Compatibilities);

/// <summary>
/// Содержит данные для создания категории.
/// </summary>
/// <param name="Name">Название категории.</param>
/// <param name="Slug">Уникальный адресный идентификатор.</param>
public sealed record CreateCategoryRequest(string Name, string Slug);

/// <summary>
/// Содержит новый статус заказа.
/// </summary>
/// <param name="Status">Статус, в который требуется перевести заказ.</param>
public sealed record ChangeOrderStatusRequest(OrderStatus Status);
