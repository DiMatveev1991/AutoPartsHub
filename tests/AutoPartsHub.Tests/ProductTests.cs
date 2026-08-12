using AutoPartsHub.Core;

namespace AutoPartsHub.Tests;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NormalizesArticle()
    {
        var product = CreateProduct(article: " oil-01 ");

        Assert.Equal("OIL-01", product.Article);
    }

    [Fact]
    public void Constructor_RejectsNonPositivePrice()
    {
        Assert.Throws<DomainException>(() => CreateProduct(price: 0));
    }

    [Fact]
    public void Reserve_DecreasesStock()
    {
        var product = CreateProduct(stock: 5);

        product.Reserve(2);

        Assert.Equal(3, product.Stock);
    }

    [Fact]
    public void Reserve_RejectsQuantityAboveStock()
    {
        var product = CreateProduct(stock: 1);

        Assert.Throws<DomainException>(() => product.Reserve(2));
        Assert.Equal(1, product.Stock);
    }

    [Fact]
    public void AddCompatibility_RejectsInvalidYearRange()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() =>
            product.AddCompatibility("Toyota", "Camry", 2020, 2019, null));
    }

    internal static Product CreateProduct(
        string article = "OIL-01",
        decimal price = 450m,
        int stock = 10) =>
        new(
            Guid.NewGuid(),
            article,
            "Масляный фильтр",
            "Описание масляного фильтра.",
            ProductCondition.New,
            price,
            stock,
            Now);
}
