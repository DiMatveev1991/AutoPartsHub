namespace AutoPartsHub.Core;

public sealed class Vehicle
{
    private Vehicle()
    {
    }

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

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Vin { get; private set; } = string.Empty;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string? Engine { get; private set; }
    public User? User { get; private set; }

    public static string NormalizeVin(string vin)
    {
        var value = vin?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(value) || value.Length != 17 ||
            value.Any(character => !char.IsLetterOrDigit(character) || character is 'I' or 'O' or 'Q'))
            throw new DomainException("VIN должен состоять из 17 допустимых символов.");
        return value;
    }

    private static string Required(string value, string name)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > 80)
            throw new DomainException($"Поле {name} обязательно и не должно превышать 80 символов.");
        return result;
    }
}
