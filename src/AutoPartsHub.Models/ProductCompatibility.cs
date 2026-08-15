namespace AutoPartsHub.Models;

/// <summary>
/// Представляет правило совместимости товара с конкретным автомобилем.
/// </summary>
/// <remarks>
/// Связь многие-к-одному: много правил совместимости относится к одному товару.
/// </remarks>
public sealed class ProductCompatibility
{
    /// <summary>
    /// Создаёт экземпляр совместимости для восстановления Entity Framework Core.
    /// </summary>
    private ProductCompatibility()
    {
    }

    /// <summary>
    /// Создаёт правило совместимости и проверяет диапазон годов выпуска.
    /// </summary>
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

    /// <summary>Получает уникальный идентификатор правила совместимости.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает внешний ключ товара; много правил совместимости относится к одному товару.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Получает марку совместимого автомобиля.</summary>
    public string Make { get; private set; } = string.Empty;

    /// <summary>Получает модель совместимого автомобиля.</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>Получает начальный год совместимости.</summary>
    public int YearFrom { get; private set; }

    /// <summary>Получает конечный год совместимости.</summary>
    public int YearTo { get; private set; }

    /// <summary>Получает обозначение совместимого двигателя.</summary>
    public string? Engine { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с товаром.</summary>
    public Product? Product { get; private set; }

    /// <summary>
    /// Проверяет обязательную строку и удаляет крайние пробелы.
    /// </summary>
    private static string Required(string value, string name)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > 80)
            throw new DomainException($"Поле {name} обязательно и не должно превышать 80 символов.");
        return result;
    }
}
