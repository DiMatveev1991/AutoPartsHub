namespace AutoPartsHub.Core;

public sealed class Product
{
    private readonly List<ProductCompatibility> _compatibilities = [];

    private Product()
    {
    }

    public Product(
        Guid categoryId,
        string article,
        string name,
        string description,
        ProductCondition condition,
        decimal price,
        int stock,
        DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        ConcurrencyToken = Guid.NewGuid();
        CategoryId = categoryId;
        Article = Required(article, nameof(article), 80).ToUpperInvariant();
        ChangeDetails(categoryId, name, description, condition, price, stock, now);
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Article { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProductCondition Condition { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid ConcurrencyToken { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Category? Category { get; private set; }
    public IReadOnlyCollection<ProductCompatibility> Compatibilities => _compatibilities;

    public void ChangeDetails(
        Guid categoryId,
        string name,
        string description,
        ProductCondition condition,
        decimal price,
        int stock,
        DateTimeOffset now)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("Категория обязательна.");
        if (price <= 0)
            throw new DomainException("Цена должна быть больше нуля.");
        if (stock < 0)
            throw new DomainException("Остаток не может быть отрицательным.");

        CategoryId = categoryId;
        Name = Required(name, nameof(name), 200);
        Description = Required(description, nameof(description), 4000);
        Condition = condition;
        Price = decimal.Round(price, 2);
        Stock = stock;
        ConcurrencyToken = Guid.NewGuid();
        UpdatedAt = now;
    }

    public void AddCompatibility(string make, string model, int yearFrom, int yearTo, string? engine)
    {
        _compatibilities.Add(new ProductCompatibility(Id, make, model, yearFrom, yearTo, engine));
    }

    public void ReplaceCompatibilities(IEnumerable<ProductCompatibilitySpec> items)
    {
        _compatibilities.Clear();
        foreach (var item in items)
            AddCompatibility(item.Make, item.Model, item.YearFrom, item.YearTo, item.Engine);
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!IsActive || Stock < quantity)
            throw new DomainException($"Недостаточно товара «{Name}» на складе.");

        Stock -= quantity;
        ConcurrencyToken = Guid.NewGuid();
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        ConcurrencyToken = Guid.NewGuid();
        UpdatedAt = now;
    }

    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}

public sealed record ProductCompatibilitySpec(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);
