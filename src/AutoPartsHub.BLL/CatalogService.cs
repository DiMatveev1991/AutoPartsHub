using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

/// <summary>
/// Выполняет пользовательские сценарии поиска и просмотра каталога.
/// </summary>
/// <param name="repository">Хранилище данных приложения.</param>
public sealed class CatalogService(IAutoPartsRepository repository)
{
    /// <summary>
    /// Возвращает отфильтрованную страницу товаров.
    /// </summary>
    public async Task<PagedResult<ProductDto>> SearchAsync(
        CatalogFilter filter,
        CancellationToken cancellationToken)
    {
        Validate(filter);
        var query = new ProductSearchQuery(
            filter.Query,
            filter.CategoryId,
            filter.Condition,
            filter.MinPrice,
            filter.MaxPrice,
            filter.Make,
            filter.Model,
            filter.Year,
            filter.Engine,
            filter.Page,
            filter.PageSize);
        var (items, totalCount) = await repository.SearchProductsAsync(query, cancellationToken);
        return new PagedResult<ProductDto>(
            items.Select(item => item.ToDto()).ToArray(),
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    /// <summary>
    /// Подбирает страницу совместимых товаров по VIN сохранённого автомобиля.
    /// </summary>
    public async Task<PagedResult<ProductDto>> SearchByVinAsync(
        string vin,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = Vehicle.NormalizeVin(vin);
        var vehicle = await repository.FindVehicleByVinAsync(normalized, cancellationToken)
            ?? throw new NotFoundException(
                "VIN пока отсутствует в локальном справочнике. Добавьте автомобиль в личном кабинете.");

        return await SearchAsync(
            new CatalogFilter(
                Make: vehicle.Make,
                Model: vehicle.Model,
                Year: vehicle.Year,
                Engine: vehicle.Engine,
                Page: page,
                PageSize: pageSize),
            cancellationToken);
    }

    /// <summary>
    /// Возвращает товар по его идентификатору.
    /// </summary>
    public async Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.FindProductAsync(id, cancellationToken)
            ?? throw new NotFoundException("Товар не найден.");
        return product.ToDto();
    }

    /// <summary>
    /// Возвращает товар по артикулу.
    /// </summary>
    public async Task<ProductDto> GetByArticleAsync(
        string article,
        CancellationToken cancellationToken)
    {
        var product = await repository.FindProductByArticleAsync(
            article.Trim().ToUpperInvariant(),
            cancellationToken) ?? throw new NotFoundException("Товар не найден.");
        return product.ToDto();
    }

    /// <summary>
    /// Возвращает доступные категории каталога.
    /// </summary>
    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var items = await repository.GetCategoriesAsync(cancellationToken);
        return items.Select(item => new CategoryDto(item.Id, item.Name, item.Slug)).ToArray();
    }

    /// <summary>
    /// Проверяет границы пагинации и диапазоны фильтра.
    /// </summary>
    private static void Validate(CatalogFilter filter)
    {
        if (filter.Page <= 0)
            throw new DomainException("Номер страницы должен быть больше нуля.");
        if (filter.PageSize is < 1 or > 100)
            throw new DomainException("Размер страницы должен быть от 1 до 100.");
        if (filter.MinPrice < 0 || filter.MaxPrice < 0 || filter.MinPrice > filter.MaxPrice)
            throw new DomainException("Некорректный диапазон цены.");
        if (filter.Year is < 1950 or > 2100)
            throw new DomainException("Некорректный год автомобиля.");
    }
}
