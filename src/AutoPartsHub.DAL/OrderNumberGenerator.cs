using AutoPartsHub.Core;

namespace AutoPartsHub.DAL;

public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    public string Next(DateTimeOffset now) =>
        $"ORD-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
