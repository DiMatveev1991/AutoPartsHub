using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает административные операции каталога и заказов.
/// </summary>
public interface IAdminService
{
    /// <summary>Создаёт категорию.</summary>
    Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken);

    /// <summary>Создаёт товар.</summary>
    Task<ProductDto> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    /// <summary>Добавляет товару правило совместимости с автомобилем.</summary>
    Task<ProductDto> AddCompatibilityAsync(
        string article,
        CompatibilityRequest request,
        CancellationToken cancellationToken);

    /// <summary>Обновляет товар.</summary>
    Task<ProductDto> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    /// <summary>Выполняет мягкое удаление товара из каталога.</summary>
    Task DeactivateProductAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Возвращает заказы всех пользователей.</summary>
    Task<IReadOnlyCollection<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken);

    /// <summary>Изменяет статус заказа.</summary>
    Task<OrderDto> ChangeOrderStatusAsync(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken);
}
