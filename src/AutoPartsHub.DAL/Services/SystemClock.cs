using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.DAL.Services;

/// <summary>
/// Предоставляет текущее системное время в UTC.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>Получает текущее системное время в UTC.</summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
