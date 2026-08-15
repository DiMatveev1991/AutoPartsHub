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
/// <param name="repository">Хранилище данных приложения.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class AdminService(IAutoPartsRepository repository, IClock clock) : IAdminService
{
    /// <summary>
    /// Создаёт новую категорию с уникальным slug.
    /// </summary>
    public async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (await repository.CategorySlugExistsAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException("Категория с таким slug уже существует.");

        var category = CategoryRules.Create(request.Name, request.Slug);
        await repository.AddCategoryAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
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
        if (await repository.ProductArticleExistsAsync(request.Article.Trim().ToUpperInvariant(), cancellationToken))
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

        await repository.AddProductAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
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
        var product = await repository.FindProductAsync(id, cancellationToken)
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

        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    /// <summary>
    /// Скрывает товар из пользовательского каталога.
    /// </summary>
    public async Task DeactivateProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        ProductRules.Deactivate(product, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает все заказы для административного просмотра.
    /// </summary>
    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(null, cancellationToken);
        return orders.Select(order => order.ToDto()).ToArray();
    }

    /// <summary>
    /// Изменяет статус заказа и создаёт уведомление для покупателя.
    /// </summary>
    public async Task<OrderDto> ChangeOrderStatusAsync(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await repository.FindOrderAsync(id, cancellationToken)
            ?? throw new NotFoundException("Заказ не найден.");

        OrderRules.ChangeStatus(order, request.Status, clock.UtcNow);
        await repository.AddNotificationAsync(
            SubscriptionRules.CreateNotification(
                order.UserId,
                "OrderStatusChanged",
                $"Статус заказа {order.OrderNumber} изменён на {request.Status}.",
                clock.UtcNow),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return order.ToDto();
    }

    /// <summary>
    /// Проверяет существование выбранной категории.
    /// </summary>
    private async Task RequireCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await repository.FindCategoryAsync(id, cancellationToken) is null)
            throw new NotFoundException("Категория не найдена.");
    }

    /// <summary>
    /// Преобразует контракт совместимости в независимый набор значений для BLL-правила.
    /// </summary>
    private static (string Make, string Model, int YearFrom, int YearTo, string? Engine) ToValues(
        CompatibilityRequest item) =>
        (item.Make, item.Model, item.YearFrom, item.YearTo, item.Engine);
}
