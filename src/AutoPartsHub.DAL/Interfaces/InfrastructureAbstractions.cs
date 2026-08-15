namespace AutoPartsHub.DAL.Interfaces;

/// <summary>
/// Предоставляет текущее время независимо от системных часов.
/// </summary>
public interface IClock
{
    /// <summary>Получает текущее время в формате UTC.</summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// Создаёт уникальные человекочитаемые номера заказов.
/// </summary>
public interface IOrderNumberGenerator
{
    /// <summary>
    /// Формирует следующий номер заказа для указанного момента времени.
    /// </summary>
    string Next(DateTimeOffset now);
}
