using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит одно правило совместимости товара с автомобилем.
/// </summary>
/// <remarks>
/// Проверка диапазона годов и обязательных строк выполняется в BLL.
/// </remarks>
public class ProductCompatibility : Entity
{
    /// <summary>Получает или задаёт внешний ключ товара.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Получает или задаёт марку автомобиля.</summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>Получает или задаёт модель автомобиля.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Получает или задаёт начальный год совместимости.</summary>
    public int YearFrom { get; set; }

    /// <summary>Получает или задаёт конечный год совместимости.</summary>
    public int YearTo { get; set; }

    /// <summary>Получает или задаёт обозначение двигателя.</summary>
    public string? Engine { get; set; }

    /// <summary>Получает или задаёт товар по связи многие-к-одному.</summary>
    public Product? Product { get; set; }
}
