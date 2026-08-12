using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

public sealed class AdminService(IAutoPartsRepository repository, IClock clock)
{
    public async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (await repository.CategorySlugExistsAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException("Категория с таким slug уже существует.");

        var category = new Category(request.Name, request.Slug);
        await repository.AddCategoryAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Slug);
    }

    public async Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        if (await repository.ProductArticleExistsAsync(request.Article.Trim().ToUpperInvariant(), cancellationToken))
            throw new ConflictException("Товар с таким артикулом уже существует.");

        var product = new Product(
            request.CategoryId,
            request.Article,
            request.Name,
            request.Description,
            request.Condition,
            request.Price,
            request.Stock,
            clock.UtcNow);
        product.ReplaceCompatibilities(request.Compatibilities.Select(ToSpec));

        await repository.AddProductAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await RequireCategoryAsync(request.CategoryId, cancellationToken);
        var product = await repository.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");

        product.ChangeDetails(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Condition,
            request.Price,
            request.Stock,
            clock.UtcNow);
        product.ReplaceCompatibilities(request.Compatibilities.Select(ToSpec));

        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    public async Task DeactivateProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        product.Deactivate(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(
        CancellationToken cancellationToken)
    {
        var orders = await repository.GetOrdersAsync(null, cancellationToken);
        return orders.Select(order => order.ToDto()).ToArray();
    }

    public async Task<OrderDto> ChangeOrderStatusAsync(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await repository.FindOrderAsync(id, cancellationToken)
            ?? throw new NotFoundException("Заказ не найден.");

        order.ChangeStatus(request.Status, clock.UtcNow);
        await repository.AddNotificationAsync(
            new Notification(
                order.UserId,
                "OrderStatusChanged",
                $"Статус заказа {order.OrderNumber} изменён на {request.Status}.",
                clock.UtcNow),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return order.ToDto();
    }

    private async Task RequireCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await repository.FindCategoryAsync(id, cancellationToken) is null)
            throw new NotFoundException("Категория не найдена.");
    }

    private static ProductCompatibilitySpec ToSpec(CompatibilityRequest item) =>
        new(item.Make, item.Model, item.YearFrom, item.YearTo, item.Engine);
}
