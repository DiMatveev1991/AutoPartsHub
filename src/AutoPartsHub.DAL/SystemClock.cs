using AutoPartsHub.Core;

namespace AutoPartsHub.DAL;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
