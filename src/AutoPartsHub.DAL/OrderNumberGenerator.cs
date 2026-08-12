using AutoPartsHub.Core;

namespace AutoPartsHub.DAL;

/// <summary>
/// Формирует номера заказов из даты и короткого случайного идентификатора.
/// </summary>
public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    /// <summary>
    /// Создаёт уникальный номер заказа в формате <c>ORD-yyyyMMdd-XXXXXXXX</c>.
    /// </summary>
    public string Next(DateTimeOffset now) =>
        $"ORD-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
