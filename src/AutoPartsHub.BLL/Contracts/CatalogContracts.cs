using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

/// <summary>
/// Описывает фильтры поиска по каталогу.
/// </summary>
/// <param name="Query">Текстовый запрос.</param>
/// <param name="CategoryId">Идентификатор категории.</param>
/// <param name="Condition">Состояние товара.</param>
/// <param name="MinPrice">Минимальная цена.</param>
/// <param name="MaxPrice">Максимальная цена.</param>
/// <param name="Make">Марка совместимого автомобиля.</param>
/// <param name="Model">Модель совместимого автомобиля.</param>
/// <param name="Year">Год выпуска совместимого автомобиля.</param>
/// <param name="Engine">Обозначение двигателя.</param>
/// <param name="Page">Номер страницы.</param>
/// <param name="PageSize">Размер страницы.</param>
public sealed record CatalogFilter(
    string? Query = null,
    Guid? CategoryId = null,
    ProductCondition? Condition = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Make = null,
    string? Model = null,
    int? Year = null,
    string? Engine = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Представляет правило совместимости товара с автомобилем.
/// </summary>
/// <param name="Make">Марка автомобиля.</param>
/// <param name="Model">Модель автомобиля.</param>
/// <param name="YearFrom">Начальный год выпуска.</param>
/// <param name="YearTo">Конечный год выпуска.</param>
/// <param name="Engine">Обозначение двигателя.</param>
public sealed record CompatibilityDto(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);

/// <summary>
/// Представляет товар каталога для выдачи пользователю.
/// </summary>
/// <param name="Id">Идентификатор товара.</param>
/// <param name="CategoryId">Идентификатор категории.</param>
/// <param name="Category">Название категории.</param>
/// <param name="Article">Артикул товара.</param>
/// <param name="Name">Название товара.</param>
/// <param name="Description">Описание товара.</param>
/// <param name="Condition">Состояние товара.</param>
/// <param name="Price">Цена товара.</param>
/// <param name="Stock">Доступный остаток.</param>
/// <param name="IsActive">Признак доступности в каталоге.</param>
/// <param name="Compatibilities">Правила совместимости.</param>
public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Category,
    string Article,
    string Name,
    string Description,
    ProductCondition Condition,
    decimal Price,
    int Stock,
    bool IsActive,
    IReadOnlyCollection<CompatibilityDto> Compatibilities);

/// <summary>
/// Представляет категорию каталога.
/// </summary>
/// <param name="Id">Идентификатор категории.</param>
/// <param name="Name">Название категории.</param>
/// <param name="Slug">Адресный идентификатор категории.</param>
public sealed record CategoryDto(Guid Id, string Name, string Slug);

/// <summary>
/// Представляет сохранённый автомобиль пользователя.
/// </summary>
/// <param name="Id">Идентификатор автомобиля.</param>
/// <param name="Vin">Нормализованный VIN.</param>
/// <param name="Make">Марка автомобиля.</param>
/// <param name="Model">Модель автомобиля.</param>
/// <param name="Year">Год выпуска.</param>
/// <param name="Engine">Обозначение двигателя.</param>
public sealed record VehicleDto(
    Guid Id,
    string Vin,
    string Make,
    string Model,
    int Year,
    string? Engine);

/// <summary>
/// Содержит данные для добавления автомобиля пользователя.
/// </summary>
/// <param name="Vin">VIN автомобиля.</param>
/// <param name="Make">Марка автомобиля.</param>
/// <param name="Model">Модель автомобиля.</param>
/// <param name="Year">Год выпуска.</param>
/// <param name="Engine">Обозначение двигателя.</param>
public sealed record AddVehicleRequest(
    string Vin,
    string Make,
    string Model,
    int Year,
    string? Engine);
