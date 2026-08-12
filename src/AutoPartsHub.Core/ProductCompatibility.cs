namespace AutoPartsHub.Core;

public sealed class ProductCompatibility
{
    private ProductCompatibility()
    {
    }

    internal ProductCompatibility(
        Guid productId,
        string make,
        string model,
        int yearFrom,
        int yearTo,
        string? engine)
    {
        if (yearFrom is < 1950 or > 2100 || yearTo < yearFrom || yearTo > 2100)
            throw new DomainException("Некорректный диапазон годов совместимости.");

        Id = Guid.NewGuid();
        ProductId = productId;
        Make = Required(make, nameof(make));
        Model = Required(model, nameof(model));
        YearFrom = yearFrom;
        YearTo = yearTo;
        Engine = string.IsNullOrWhiteSpace(engine) ? null : engine.Trim();
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int YearFrom { get; private set; }
    public int YearTo { get; private set; }
    public string? Engine { get; private set; }
    public Product? Product { get; private set; }

    private static string Required(string value, string name)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > 80)
            throw new DomainException($"Поле {name} обязательно и не должно превышать 80 символов.");
        return result;
    }
}
