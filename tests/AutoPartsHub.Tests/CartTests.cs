using AutoPartsHub.Core;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет добавление, объединение и удаление позиций корзины.
/// </summary>
public sealed class CartTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет добавление новой позиции.</summary>
    [Fact]
    public void Add_AddsNewLine()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var product = ProductTests.CreateProduct(stock: 5);

        cart.Add(product, 2, Now);

        var item = Assert.Single(cart.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    /// <summary>Проверяет объединение одинаковых товаров.</summary>
    [Fact]
    public void Add_MergesSameProduct()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var product = ProductTests.CreateProduct(stock: 5);

        cart.Add(product, 1, Now);
        cart.Add(product, 2, Now);

        Assert.Equal(3, Assert.Single(cart.Items).Quantity);
    }

    /// <summary>Проверяет неизменность корзины при превышении остатка.</summary>
    [Fact]
    public void Add_DoesNotMutateCartWhenStockExceeded()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var product = ProductTests.CreateProduct(stock: 2);
        cart.Add(product, 1, Now);

        Assert.Throws<DomainException>(() => cart.Add(product, 2, Now));
        Assert.Equal(1, Assert.Single(cart.Items).Quantity);
    }

    /// <summary>Проверяет удаление товарной позиции.</summary>
    [Fact]
    public void Remove_DeletesLine()
    {
        var cart = new Cart(Guid.NewGuid(), Now);
        var product = ProductTests.CreateProduct();
        cart.Add(product, 1, Now);

        cart.Remove(product.Id, Now);

        Assert.Empty(cart.Items);
    }
}
