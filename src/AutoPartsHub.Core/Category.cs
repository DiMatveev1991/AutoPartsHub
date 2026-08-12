namespace AutoPartsHub.Core;

public sealed class Category
{
    private Category()
    {
    }

    public Category(string name, string slug)
    {
        Id = Guid.NewGuid();
        Rename(name, slug);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public void Rename(string name, string slug)
    {
        Name = Required(name, nameof(name), 120);
        Slug = Required(slug, nameof(slug), 120).ToLowerInvariant();
    }

    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}
