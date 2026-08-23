using AutoPartsHub.BLL;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет правила BLL для товара и складского остатка.
/// </summary>
/// <remarks>
/// Тесты вызывают BLL-правила, а не методы Product: модель намеренно хранит
/// только данные, как модели в DeliveryApp.
/// </remarks>
public sealed class ProductTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет нормализацию артикула при создании товара.</summary>
    [Fact]
    public void Create_NormalizesArticle()
    {
        var product = CreateProduct(article: " oil-01 ");

        Assert.Equal("OIL-01", product.Article);
    }

    /// <summary>Проверяет запрет нулевой и отрицательной цены.</summary>
    [Fact]
    public void Create_RejectsNonPositivePrice()
    {
        Assert.Throws<DomainException>(() => CreateProduct(price: 0));
    }

    /// <summary>Проверяет уменьшение остатка при резервировании.</summary>
    [Fact]
    public void Reserve_DecreasesStock()
    {
        var product = CreateProduct(stock: 5);

        ProductRules.Reserve(product, 2);

        Assert.Equal(3, product.Stock);
    }

    /// <summary>Проверяет запрет резервирования сверх доступного остатка.</summary>
    [Fact]
    public void Reserve_RejectsQuantityAboveStock()
    {
        var product = CreateProduct(stock: 1);

        Assert.Throws<DomainException>(() => ProductRules.Reserve(product, 2));
        Assert.Equal(1, product.Stock);
    }

    /// <summary>Проверяет валидацию диапазона годов совместимости.</summary>
    [Fact]
    public void ReplaceCompatibilities_RejectsInvalidYearRange()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => ProductRules.ReplaceCompatibilities(
            product,
            [("Toyota", "Camry", 2020, 2019, null)]));
    }

    /// <summary>Проверяет добавление совместимости без удаления существующих правил.</summary>
    [Fact]
    public void AddCompatibility_AppendsRule()
    {
        var product = CreateProduct();

        ProductRules.AddCompatibility(
            product,
            ("Toyota", "Camry", 2015, 2022, "2.5"));

        var compatibility = Assert.Single(product.Compatibilities);
        Assert.Equal("Toyota", compatibility.Make);
        Assert.Equal("Camry", compatibility.Model);
        Assert.Equal(2015, compatibility.YearFrom);
        Assert.Equal(2022, compatibility.YearTo);
        Assert.Equal("2.5", compatibility.Engine);
    }

    /// <summary>Проверяет запрет дублирования одинаковой совместимости.</summary>
    [Fact]
    public void AddCompatibility_RejectsDuplicate()
    {
        var product = CreateProduct();
        ProductRules.AddCompatibility(
            product,
            ("Toyota", "Camry", 2015, 2022, "2.5"));

        Assert.Throws<DomainException>(() => ProductRules.AddCompatibility(
            product,
            ("toyota", "camry", 2015, 2022, "2.5")));
    }

    /// <summary>Создаёт корректный товар через то же BLL-правило, что использует сервис.</summary>
    internal static Product CreateProduct(
        string article = "OIL-01",
        decimal price = 450m,
        int stock = 10) =>
        ProductRules.Create(
            Guid.NewGuid(),
            article,
            "Масляный фильтр",
            "Описание масляного фильтра.",
            ProductCondition.New,
            price,
            stock,
            Now);
}
