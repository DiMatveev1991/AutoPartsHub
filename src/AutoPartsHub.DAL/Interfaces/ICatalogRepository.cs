using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>
/// Описывает проверенные параметры запроса, которые DAL преобразует в LINQ к базе данных.
/// </summary>
/// <remarks>
/// Это не пользовательский DTO: тип принадлежит контракту хранилища и содержит
/// уже проверенные BLL значения, необходимые только для построения запроса.
/// </remarks>
/// <param name="Text">Строка поиска по названию, описанию или артикулу.</param>
/// <param name="CategoryId">Идентификатор категории.</param>
/// <param name="Condition">Состояние товара.</param>
/// <param name="MinPrice">Минимальная цена.</param>
/// <param name="MaxPrice">Максимальная цена.</param>
/// <param name="Make">Марка совместимого автомобиля.</param>
/// <param name="Model">Модель совместимого автомобиля.</param>
/// <param name="Year">Год выпуска совместимого автомобиля.</param>
/// <param name="Engine">Обозначение двигателя.</param>
/// <param name="Page">Номер страницы, начиная с единицы.</param>
/// <param name="PageSize">Количество товаров на странице.</param>
public sealed record ProductSearchQuery(
    string? Text = null,
    Guid? CategoryId = null,
    ProductCondition? Condition = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Make = null,
    string? Model = null,
    int? Year = null,
    string? Engine = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>Определяет операции чтения и изменения каталога.</summary>
public interface ICatalogRepository
{
    /// <summary>Возвращает страницу товаров и общее количество результатов.</summary>
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
        ProductSearchQuery query,
        CancellationToken cancellationToken);

    /// <summary>Находит товар по идентификатору.</summary>
    Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Находит товар по нормализованному артикулу.</summary>
    Task<Product?> FindProductByArticleAsync(string article, CancellationToken cancellationToken);

    /// <summary>Проверяет существование товара с указанным артикулом.</summary>
    Task<bool> ProductArticleExistsAsync(string article, CancellationToken cancellationToken);

    /// <summary>Добавляет товар в контекст хранения.</summary>
    Task AddProductAsync(Product product, CancellationToken cancellationToken);

    /// <summary>Возвращает все категории каталога.</summary>
    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken);

    /// <summary>Находит категорию по идентификатору.</summary>
    Task<Category?> FindCategoryAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Находит категорию по нормализованному slug.</summary>
    Task<Category?> FindCategoryBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Проверяет существование категории с указанным slug.</summary>
    Task<bool> CategorySlugExistsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Добавляет категорию в контекст хранения.</summary>
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken);
}
