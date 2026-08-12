namespace AutoPartsHub.Core;

/// <summary>
/// Описывает автомобиль пользователя, для которого подбираются совместимые запчасти.
/// </summary>
public sealed class Vehicle
{
    /// <summary>
    /// Создаёт экземпляр автомобиля для восстановления Entity Framework Core.
    /// </summary>
    private Vehicle()
    {
    }

    /// <summary>
    /// Создаёт автомобиль пользователя и нормализует его VIN.
    /// </summary>
    public Vehicle(Guid userId, string vin, string make, string model, int year, string? engine)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Пользователь обязателен.");
        if (year is < 1950 or > 2100)
            throw new DomainException("Некорректный год автомобиля.");

        Id = Guid.NewGuid();
        UserId = userId;
        Vin = NormalizeVin(vin);
        Make = Required(make, nameof(make));
        Model = Required(model, nameof(model));
        Year = year;
        Engine = string.IsNullOrWhiteSpace(engine) ? null : engine.Trim();
    }

    /// <summary>Получает уникальный идентификатор автомобиля.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает идентификатор владельца автомобиля.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает нормализованный VIN автомобиля.</summary>
    public string Vin { get; private set; } = string.Empty;

    /// <summary>Получает марку автомобиля.</summary>
    public string Make { get; private set; } = string.Empty;

    /// <summary>Получает модель автомобиля.</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>Получает год выпуска автомобиля.</summary>
    public int Year { get; private set; }

    /// <summary>Получает обозначение двигателя, если оно указано.</summary>
    public string? Engine { get; private set; }

    /// <summary>Получает владельца автомобиля при загрузке связи из базы данных.</summary>
    public User? User { get; private set; }

    /// <summary>
    /// Приводит VIN к единому формату и проверяет допустимые символы.
    /// </summary>
    public static string NormalizeVin(string vin)
    {
        var value = vin?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(value) || value.Length != 17 ||
            value.Any(character => !char.IsLetterOrDigit(character) || character is 'I' or 'O' or 'Q'))
            throw new DomainException("VIN должен состоять из 17 допустимых символов.");
        return value;
    }

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
