using AutoPartsHub.BLL;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.BLL.Services;

/// <summary>
/// Выполняет административные сценарии каталога и заказов.
/// </summary>
/// <param name="catalog">Хранилище каталога.</param>
/// <param name="orders">Хранилище заказов.</param>
/// <param name="notifications">Хранилище уведомлений.</param>
/// <param name="unitOfWork">Граница сохранения изменений.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class AdminService(
    ICatalogRepository catalog,
    IOrderRepository orders,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    IClock clock) : IAdminService
{
    /// <summary>
    /// Создаёт новую категорию с уникальным slug.
    /// </summary>
    public async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (await catalog.CategorySlugExistsAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException("Категория с таким slug уже существует.");

        var category = CategoryRules.Create(request.Name, request.Slug);
        await catalog.AddCategoryAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Slug);
    }

    /// <summary>
    /// Создаёт новый товар вместе с правилами совместимости.
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        if (await catalog.ProductArticleExistsAsync(request.Article.Trim().ToUpperInvariant(), cancellationToken))
            throw new ConflictException("Товар с таким артикулом уже существует.");

        var product = ProductRules.Create(
            request.CategoryId,
            request.Article,
            request.Name,
            request.Description,
            request.Condition,
            request.Price,
            request.Stock,
            clock.UtcNow);
        ProductRules.ReplaceCompatibilities(product, request.Compatibilities.Select(ToValues));

        await catalog.AddProductAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Добавляет существующему товару проверенное правило совместимости.
    /// </summary>
    public async Task<ProductDto> AddCompatibilityAsync(
        string article,
        CompatibilityRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(article))
            throw new DomainException("Артикул обязателен.");

        var product = await catalog.FindProductByArticleAsync(
            article.Trim().ToUpperInvariant(),
            cancellationToken) ?? throw new NotFoundException("Товар не найден.");

        ProductRules.AddCompatibility(product, ToValues(request));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Изменяет цену и остаток товара, найденного по артикулу.
    /// </summary>
    public async Task<ProductDto> UpdateProductPriceAndStockAsync(
        string article,
        decimal price,
        int stock,
        CancellationToken cancellationToken)
    {
        var product = await RequireProductByArticleAsync(article, cancellationToken);
        ProductRules.UpdatePriceAndStock(product, price, stock, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Выполняет мягкое удаление товара, найденного по артикулу.
    /// </summary>
    public async Task DeactivateProductByArticleAsync(
        string article,
        CancellationToken cancellationToken)
    {
        var product = await RequireProductByArticleAsync(article, cancellationToken);
        ProductRules.Deactivate(product, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает мягко удалённый товар в каталог.
    /// </summary>
    public async Task<ProductDto> ActivateProductByArticleAsync(
        string article,
        CancellationToken cancellationToken)
    {
        var product = await RequireProductByArticleAsync(article, cancellationToken);
        ProductRules.Activate(product, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Обновляет характеристики и совместимость существующего товара.
    /// </summary>
    public async Task<ProductDto> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        var product = await catalog.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");

        ProductRules.Update(
            product,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Condition,
            request.Price,
            request.Stock,
            clock.UtcNow);
        ProductRules.ReplaceCompatibilities(product, request.Compatibilities.Select(ToValues));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Скрывает товар из пользовательского каталога.
    /// </summary>
    public async Task DeactivateProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await catalog.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        ProductRules.Deactivate(product, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает все заказы для административного просмотра.
    /// </summary>
    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken)
    {
        var items = await orders.GetAsync(null, cancellationToken);
        return items.Select(order => order.ToDto()).ToArray();
    }

    /// <summary>
    /// Изменяет статус заказа и создаёт уведомление для покупателя.
    /// </summary>
    public async Task<OrderDto> ChangeOrderStatusAsync(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orders.FindAsync(id, cancellationToken)
            ?? throw new NotFoundException("Заказ не найден.");

        OrderRules.ChangeStatus(order, request.Status, clock.UtcNow);
        await notifications.AddAsync(
            SubscriptionRules.CreateNotification(
                order.UserId,
                "OrderStatusChanged",
                $"Статус заказа {order.OrderNumber} изменён на {request.Status}.",
                clock.UtcNow),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToDto();
    }

    /// <summary>
    /// Находит товар по обязательному нормализованному артикулу.
    /// </summary>
    private async Task<Product> RequireProductByArticleAsync(
        string article,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(article))
            throw new DomainException("Артикул обязателен.");

        return await catalog.FindProductByArticleAsync(
            article.Trim().ToUpperInvariant(),
            cancellationToken) ?? throw new NotFoundException("Товар не найден.");
    }

    /// <summary>
    /// Проверяет существование выбранной категории.
    /// </summary>
    private async Task RequireCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await catalog.FindCategoryAsync(id, cancellationToken) is null)
            throw new NotFoundException("Категория не найдена.");
    }

    /// <summary>
    /// Преобразует контракт совместимости в независимый набор значений для BLL-правила.
    /// </summary>
    private static (string Make, string Model, int YearFrom, int YearTo, string? Engine) ToValues(
        CompatibilityRequest item) =>
        (item.Make, item.Model, item.YearFrom, item.YearTo, item.Engine);
}
