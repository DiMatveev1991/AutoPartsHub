using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

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

public sealed record CompatibilityDto(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);

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

public sealed record CategoryDto(Guid Id, string Name, string Slug);

public sealed record VehicleDto(
    Guid Id,
    string Vin,
    string Make,
    string Model,
    int Year,
    string? Engine);

public sealed record AddVehicleRequest(
    string Vin,
    string Make,
    string Model,
    int Year,
    string? Engine);
