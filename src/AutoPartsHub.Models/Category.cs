using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит данные категории каталога.
/// </summary>
/// <remarks>
/// Проверка обязательных полей и нормализация slug выполняются в BLL.
/// </remarks>
public class Category : Entity
{
    /// <summary>Получает или задаёт отображаемое название.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Получает или задаёт нормализованный уникальный slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Получает или задаёт товары по связи один-ко-многим.</summary>
    public ICollection<Product> Products { get; set; } = [];
}
