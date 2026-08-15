using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет оформление заказа, снимок цены и переходы статусов.
/// </summary>
public sealed class OrderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет резервирование остатка и сохранение цены в заказе.</summary>
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

    /// <summary>Проверяет запрет оформления пустого заказа.</summary>
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

    /// <summary>Проверяет разрешённую последовательность переходов статуса.</summary>
    [Fact]
    public void ChangeStatus_AllowsConfiguredTransition()
    {
        var order = CreateOnlineOrder();

        order.ChangeStatus(OrderStatus.Paid, Now.AddMinutes(1));
        order.ChangeStatus(OrderStatus.Processing, Now.AddMinutes(2));

        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    /// <summary>Проверяет запрет пропуска обязательных этапов заказа.</summary>
    [Fact]
    public void ChangeStatus_RejectsSkippingStages()
    {
        var order = CreateOnlineOrder();

        Assert.Throws<DomainException>(() =>
            order.ChangeStatus(OrderStatus.Delivered, Now.AddMinutes(1)));
    }

    /// <summary>
    /// Создаёт заказ, ожидающий онлайн-оплаты.
    /// </summary>
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
