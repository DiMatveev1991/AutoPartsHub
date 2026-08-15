using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит сведения об автомобиле пользователя для подбора запчастей.
/// </summary>
/// <remarks>
/// Нормализация VIN и проверка года находятся в BLL, поэтому модель остаётся
/// простым объектом данных и подходит для восстановления Entity Framework Core.
/// </remarks>
public class Vehicle : Entity
{
    /// <summary>Получает или задаёт внешний ключ владельца.</summary>
    public Guid UserId { get; set; }

    /// <summary>Получает или задаёт нормализованный VIN.</summary>
    public string Vin { get; set; } = string.Empty;

    /// <summary>Получает или задаёт марку автомобиля.</summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>Получает или задаёт модель автомобиля.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Получает или задаёт год выпуска.</summary>
    public int Year { get; set; }

    /// <summary>Получает или задаёт обозначение двигателя.</summary>
    public string? Engine { get; set; }

    /// <summary>Получает или задаёт владельца по связи многие-к-одному.</summary>
    public User? User { get; set; }
}
