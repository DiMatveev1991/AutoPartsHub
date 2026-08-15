using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет нормализацию VIN и условия товарных подписок.
/// </summary>
public sealed class VehicleAndSubscriptionTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет нормализацию корректного VIN.</summary>
    [Fact]
    public void Vehicle_NormalizesValidVin()
    {
        var vehicle = new Vehicle(
            Guid.NewGuid(),
            " jt2bg22k1v0123456 ",
            "Toyota",
            "Camry",
            2015,
            "2.5");

        Assert.Equal("JT2BG22K1V0123456", vehicle.Vin);
    }

    /// <summary>Проверяет отклонение VIN недопустимого формата.</summary>
    [Theory]
    [InlineData("SHORT")]
    [InlineData("JT2BG22K1I0123456")]
    [InlineData("JT2BG22K1O0123456")]
    public void Vehicle_RejectsInvalidVin(string vin)
    {
        Assert.Throws<DomainException>(() =>
            new Vehicle(Guid.NewGuid(), vin, "Toyota", "Camry", 2015, "2.5"));
    }

    /// <summary>Проверяет срабатывание подписки при появлении остатка.</summary>
    [Fact]
    public void BackInStockSubscription_TriggersWhenStockPositive()
    {
        var product = ProductTests.CreateProduct(stock: 1);
        var subscription = new ProductSubscription(
            Guid.NewGuid(),
            product.Id,
            SubscriptionType.BackInStock,
            null,
            Now);

        Assert.True(subscription.IsTriggeredBy(product));
        subscription.Complete();
        Assert.False(subscription.IsTriggeredBy(product));
    }

    /// <summary>Проверяет обязательность целевой цены для подписки на снижение.</summary>
    [Fact]
    public void PriceDropSubscription_RequiresTargetPrice()
    {
        Assert.Throws<DomainException>(() =>
            new ProductSubscription(
                Guid.NewGuid(),
                Guid.NewGuid(),
                SubscriptionType.PriceDrop,
                null,
                Now));
    }
}
