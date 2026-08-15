using AutoPartsHub.BLL;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>Проверяет BLL-правила оформления заказа, снимка цены и переходов статусов.</summary>
public sealed class OrderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет резервирование остатка и сохранение цены в заказе.</summary>
    [Fact]
    public void Create_SnapshotsPriceAndReservesStock()
    {
        var product = ProductTests.CreateProduct(price: 500m, stock: 4);

        var order = CreateOrder(PaymentMethod.CashOnDelivery, [(product, 2)]);

        Assert.Equal(1000m, order.Total);
        Assert.Equal(2, product.Stock);
        Assert.Equal(500m, Assert.Single(order.Items).UnitPrice);
        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    /// <summary>Проверяет запрет оформления пустого заказа.</summary>
    [Fact]
    public void Create_RejectsEmptyOrder()
    {
        Assert.Throws<DomainException>(() =>
            CreateOrder(PaymentMethod.CardOnline, []));
    }

    /// <summary>Проверяет разрешённую последовательность переходов статуса.</summary>
    [Fact]
    public void ChangeStatus_AllowsConfiguredTransition()
    {
        var order = CreateOnlineOrder();

        OrderRules.ChangeStatus(order, OrderStatus.Paid, Now.AddMinutes(1));
        OrderRules.ChangeStatus(order, OrderStatus.Processing, Now.AddMinutes(2));

        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    /// <summary>Проверяет запрет пропуска обязательных этапов заказа.</summary>
    [Fact]
    public void ChangeStatus_RejectsSkippingStages()
    {
        var order = CreateOnlineOrder();

        Assert.Throws<DomainException>(() =>
            OrderRules.ChangeStatus(order, OrderStatus.Delivered, Now.AddMinutes(1)));
    }

    /// <summary>Создаёт заказ, ожидающий онлайн-оплаты.</summary>
    private static Order CreateOnlineOrder() =>
        CreateOrder(PaymentMethod.CardOnline, [(ProductTests.CreateProduct(), 1)]);

    /// <summary>Создаёт заказ через правило BLL с повторно используемыми контактами.</summary>
    private static Order CreateOrder(
        PaymentMethod paymentMethod,
        IReadOnlyCollection<(Product Product, int Quantity)> lines) =>
        OrderRules.Create(
            Guid.NewGuid(),
            "ORD-1",
            "Иван",
            "+79990000000",
            "Москва",
            DeliveryMethod.Courier,
            paymentMethod,
            lines,
            Now);
}
