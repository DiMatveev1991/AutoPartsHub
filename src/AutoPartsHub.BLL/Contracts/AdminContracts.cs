using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

public sealed record CompatibilityRequest(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Article,
    string Name,
    string Description,
    ProductCondition Condition,
    decimal Price,
    int Stock,
    IReadOnlyCollection<CompatibilityRequest> Compatibilities);

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Description,
    ProductCondition Condition,
    decimal Price,
    int Stock,
    IReadOnlyCollection<CompatibilityRequest> Compatibilities);

public sealed record CreateCategoryRequest(string Name, string Slug);

public sealed record ChangeOrderStatusRequest(OrderStatus Status);
