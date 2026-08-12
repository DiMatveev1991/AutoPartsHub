namespace AutoPartsHub.Core;

/// <summary>
/// Представляет товар каталога с ценой, остатком и правилами совместимости.
/// </summary>
public sealed class Product
{
    private readonly List<ProductCompatibility> _compatibilities = [];

    /// <summary>
    /// Создаёт экземпляр товара для восстановления Entity Framework Core.
    /// </summary>
    private Product()
    {
    }

    /// <summary>
    /// Создаёт товар и устанавливает его исходные характеристики.
    /// </summary>
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

    /// <summary>Получает уникальный идентификатор товара.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает идентификатор категории товара.</summary>
    public Guid CategoryId { get; private set; }

    /// <summary>Получает нормализованный артикул товара.</summary>
    public string Article { get; private set; } = string.Empty;

    /// <summary>Получает название товара.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Получает подробное описание товара.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Получает состояние товара.</summary>
    public ProductCondition Condition { get; private set; }

    /// <summary>Получает текущую цену товара.</summary>
    public decimal Price { get; private set; }

    /// <summary>Получает доступный остаток товара.</summary>
    public int Stock { get; private set; }

    /// <summary>Указывает, доступен ли товар в каталоге.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Получает токен оптимистичной блокировки товара.</summary>
    public Guid ConcurrencyToken { get; private set; }

    /// <summary>Получает дату и время создания товара.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Получает дату и время последнего изменения товара.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Получает категорию товара при загрузке связи из базы данных.</summary>
    public Category? Category { get; private set; }

    /// <summary>Получает список правил совместимости товара с автомобилями.</summary>
    public IReadOnlyCollection<ProductCompatibility> Compatibilities => _compatibilities;

    /// <summary>
    /// Обновляет изменяемые характеристики товара и токен конкурентного доступа.
    /// </summary>
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
        // Новый токен позволяет EF Core обнаружить параллельное изменение товара.
        ConcurrencyToken = Guid.NewGuid();
        UpdatedAt = now;
    }

    /// <summary>
    /// Добавляет правило совместимости товара с автомобилем.
    /// </summary>
    public void AddCompatibility(string make, string model, int yearFrom, int yearTo, string? engine)
    {
        _compatibilities.Add(new ProductCompatibility(Id, make, model, yearFrom, yearTo, engine));
    }

    /// <summary>
    /// Полностью заменяет набор правил совместимости товара.
    /// </summary>
    public void ReplaceCompatibilities(IEnumerable<ProductCompatibilitySpec> items)
    {
        _compatibilities.Clear();
        foreach (var item in items)
            AddCompatibility(item.Make, item.Model, item.YearFrom, item.YearTo, item.Engine);
    }

    /// <summary>
    /// Резервирует указанное количество товара для заказа.
    /// </summary>
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!IsActive || Stock < quantity)
            throw new DomainException($"Недостаточно товара «{Name}» на складе.");

        Stock -= quantity;
        ConcurrencyToken = Guid.NewGuid();
    }

    /// <summary>
    /// Скрывает товар из каталога и обновляет данные конкурентного доступа.
    /// </summary>
    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        ConcurrencyToken = Guid.NewGuid();
        UpdatedAt = now;
    }

    /// <summary>
    /// Проверяет обязательную строку, удаляет крайние пробелы и контролирует длину.
    /// </summary>
    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}

/// <summary>
/// Описывает входные данные правила совместимости товара с автомобилем.
/// </summary>
/// <param name="Make">Марка автомобиля.</param>
/// <param name="Model">Модель автомобиля.</param>
/// <param name="YearFrom">Начальный год выпуска.</param>
/// <param name="YearTo">Конечный год выпуска.</param>
/// <param name="Engine">Обозначение двигателя или <see langword="null"/>.</param>
public sealed record ProductCompatibilitySpec(
    string Make,
    string Model,
    int YearFrom,
    int YearTo,
    string? Engine);
