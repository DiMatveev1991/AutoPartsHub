using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает пользовательские операции каталога автозапчастей.
/// </summary>
public interface ICatalogService
{
    /// <summary>Выполняет поиск товаров по фильтру.</summary>
    Task<PagedResult<ProductDto>> SearchAsync(
        CatalogFilter filter,
        CancellationToken cancellationToken);

    /// <summary>Подбирает товары по VIN сохранённого автомобиля.</summary>
    Task<PagedResult<ProductDto>> SearchByVinAsync(
        string vin,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>Возвращает товар по идентификатору.</summary>
    Task<ProductDto> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Возвращает товар по артикулу.</summary>
    Task<ProductDto> GetByArticleAsync(string article, CancellationToken cancellationToken);

    /// <summary>Возвращает категории каталога.</summary>
    Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
}
