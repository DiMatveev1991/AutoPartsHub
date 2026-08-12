using AutoPartsHub.Core;

namespace AutoPartsHub.DAL;

/// <summary>
/// Предоставляет текущее системное время в UTC.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>Получает текущее системное время в UTC.</summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
