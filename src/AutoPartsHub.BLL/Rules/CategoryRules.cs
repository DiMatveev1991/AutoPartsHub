using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Создаёт нормализованные категории каталога.
/// </summary>
internal static class CategoryRules
{
    /// <summary>Создаёт категорию с нормализованным уникальным slug.</summary>
    internal static Category Create(string name, string slug) => new()
    {
        Id = Guid.NewGuid(),
        Name = ValidationRules.Required(name, nameof(name), 120),
        Slug = ValidationRules.Required(slug, nameof(slug), 120).ToLowerInvariant()
    };
}
