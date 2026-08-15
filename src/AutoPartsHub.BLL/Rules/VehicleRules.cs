using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Проверяет и нормализует сведения об автомобиле пользователя.
/// </summary>
internal static class VehicleRules
{
    /// <summary>Создаёт проверенную модель автомобиля.</summary>
    internal static Vehicle Create(
        Guid userId,
        string vin,
        string make,
        string model,
        int year,
        string? engine)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Пользователь обязателен.");
        if (year is < 1950 or > 2100)
            throw new DomainException("Некорректный год автомобиля.");

        return new Vehicle
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Vin = NormalizeVin(vin),
            Make = ValidationRules.Required(make, nameof(make), 80),
            Model = ValidationRules.Required(model, nameof(model), 80),
            Year = year,
            Engine = ValidationRules.Optional(engine)
        };
    }

    /// <summary>Приводит VIN к формату хранения и отклоняет запрещённые символы.</summary>
    internal static string NormalizeVin(string vin)
    {
        var value = vin?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(value) || value.Length != 17 ||
            value.Any(character =>
                !char.IsLetterOrDigit(character) || character is 'I' or 'O' or 'Q'))
        {
            throw new DomainException("VIN должен состоять из 17 допустимых символов.");
        }

        return value;
    }
}
