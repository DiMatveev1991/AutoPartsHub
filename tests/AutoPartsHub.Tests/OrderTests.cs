using AutoPartsHub.Core;

namespace AutoPartsHub.Tests;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_SnapshotsPriceAndReservesStock()
    {
        var product = ProductTests.CreateProduct(price: 500m, stock: 4);

        var order = Order.Create(
            Guid.NewGuid(),
            "ORD-1",
            "Иван",
            "+79990000000",
            "Москва",
            DeliveryMethod.Courier,
            PaymentMethod.CashOnDelivery,
            [(product, 2)],
            Now);

        Assert.Equal(1000m, order.Total);
        Assert.Equal(2, product.Stock);
        Assert.Equal(500m, Assert.Single(order.Items).UnitPrice);
        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void Create_RejectsEmptyOrder()
    {
        Assert.Throws<DomainException>(() => Order.Create(
            Guid.NewGuid(),
            "ORD-1",
            "Иван",
            "+79990000000",
            "Москва",
            DeliveryMethod.Courier,
            PaymentMethod.CardOnline,
            [],
            Now));
    }

    [Fact]
    public void ChangeStatus_AllowsConfiguredTransition()
    {
        var order = CreateOnlineOrder();

        order.ChangeStatus(OrderStatus.Paid, Now.AddMinutes(1));
        order.ChangeStatus(OrderStatus.Processing, Now.AddMinutes(2));

        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void ChangeStatus_RejectsSkippingStages()
    {
        var order = CreateOnlineOrder();

        Assert.Throws<DomainException>(() =>
            order.ChangeStatus(OrderStatus.Delivered, Now.AddMinutes(1)));
    }

    private static Order CreateOnlineOrder()
    {
        var product = ProductTests.CreateProduct();
        return Order.Create(
            Guid.NewGuid(),
            "ORD-1",
            "Иван",
            "+79990000000",
            "Москва",
            DeliveryMethod.Courier,
            PaymentMethod.CardOnline,
            [(product, 1)],
            Now);
    }
}
