using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Управляет жизненным циклом товара, остатком и правилами совместимости.
/// </summary>
/// <remarks>
/// Все изменения сгруппированы здесь, а Product остаётся контейнером данных.
/// Сервис администратора координирует сценарий, но не дублирует правила товара.
/// </remarks>
internal static class ProductRules
{
    /// <summary>Создаёт активный товар с исходным токеном конкурентного доступа.</summary>
    internal static Product Create(
        Guid categoryId,
        string article,
        string name,
        string description,
        ProductCondition condition,
        decimal price,
        int stock,
        DateTimeOffset now)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Article = ValidationRules.Required(article, nameof(article), 80).ToUpperInvariant(),
            IsActive = true,
            CreatedAt = now
        };
        Update(product, categoryId, name, description, condition, price, stock, now);
        return product;
    }

    /// <summary>Обновляет изменяемые характеристики и токен оптимистичной блокировки.</summary>
    internal static void Update(
        Product product,
        Guid categoryId,
        string name,
        string description,
        ProductCondition condition,
        decimal price,
        int stock,
        DateTimeOffset now)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("Категория обязательна.");
        if (price <= 0)
            throw new DomainException("Цена должна быть больше нуля.");
        if (stock < 0)
            throw new DomainException("Остаток не может быть отрицательным.");

        product.CategoryId = categoryId;
        product.Name = ValidationRules.Required(name, nameof(name), 200);
        product.Description = ValidationRules.Required(description, nameof(description), 4000);
        product.Condition = condition;
        product.Price = decimal.Round(price, 2);
        product.Stock = stock;
        // EF сравнит это значение с исходным и обнаружит параллельное изменение товара.
        product.ConcurrencyToken = Guid.NewGuid();
        product.UpdatedAt = now;
    }

    /// <summary>Полностью заменяет совместимость после проверки всего нового набора.</summary>
    internal static void ReplaceCompatibilities(
        Product product,
        IEnumerable<(string Make, string Model, int YearFrom, int YearTo, string? Engine)> items)
    {
        // Сначала строится новый список. Если одна строка ошибочна, старая
        // совместимость не очищается и отслеживаемая сущность остаётся целой.
        var replacements = items.Select(item => CreateCompatibility(
            product.Id,
            item.Make,
            item.Model,
            item.YearFrom,
            item.YearTo,
            item.Engine)).ToArray();

        product.Compatibilities.Clear();
        foreach (var compatibility in replacements)
            product.Compatibilities.Add(compatibility);
    }

    /// <summary>Резервирует остаток товара для оформляемого заказа.</summary>
    internal static void Reserve(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!product.IsActive || product.Stock < quantity)
            throw new DomainException($"Недостаточно товара «{product.Name}» на складе.");

        product.Stock -= quantity;
        product.ConcurrencyToken = Guid.NewGuid();
    }

    /// <summary>Скрывает товар из каталога без физического удаления истории.</summary>
    internal static void Deactivate(Product product, DateTimeOffset now)
    {
        product.IsActive = false;
        product.ConcurrencyToken = Guid.NewGuid();
        product.UpdatedAt = now;
    }

    /// <summary>Создаёт одно проверенное правило совместимости.</summary>
    private static ProductCompatibility CreateCompatibility(
        Guid productId,
        string make,
        string model,
        int yearFrom,
        int yearTo,
        string? engine)
    {
        if (yearFrom is < 1950 or > 2100 || yearTo < yearFrom || yearTo > 2100)
            throw new DomainException("Некорректный диапазон годов совместимости.");

        return new ProductCompatibility
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Make = ValidationRules.Required(make, nameof(make), 80),
            Model = ValidationRules.Required(model, nameof(model), 80),
            YearFrom = yearFrom,
            YearTo = yearTo,
            Engine = ValidationRules.Optional(engine)
        };
    }
}
