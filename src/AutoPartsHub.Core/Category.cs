namespace AutoPartsHub.Core;

/// <summary>
/// Представляет категорию товаров каталога.
/// </summary>
/// <remarks>
/// Связь один-ко-многим: одна категория содержит много товаров. Обратная
/// коллекция товаров не объявлена и настроена в DAL через Fluent API.
/// </remarks>
public sealed class Category
{
    /// <summary>
    /// Создаёт экземпляр категории для восстановления Entity Framework Core.
    /// </summary>
    private Category()
    {
    }

    /// <summary>
    /// Создаёт категорию с названием и уникальным адресным идентификатором.
    /// </summary>
    public Category(string name, string slug)
    {
        Id = Guid.NewGuid();
        Rename(name, slug);
    }

    /// <summary>Получает уникальный идентификатор категории.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает отображаемое название категории.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Получает нормализованный slug категории.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Изменяет название и slug категории после проверки значений.
    /// </summary>
    public void Rename(string name, string slug)
    {
        Name = Required(name, nameof(name), 120);
        Slug = Required(slug, nameof(slug), 120).ToLowerInvariant();
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
