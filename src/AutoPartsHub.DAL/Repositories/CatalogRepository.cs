using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует запросы каталога через EF Core.</summary>
internal sealed class CatalogRepository(AutoPartsDbContext db) : ICatalogRepository
{
    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
        ProductSearchQuery filter,
        CancellationToken cancellationToken)
    {
        // Каталог используется только для чтения, поэтому AsNoTracking уменьшает
        // расходы Change Tracker. SplitQuery исключает размножение строк из-за коллекций.
        var query = db.Products
            .AsNoTracking()
            .Where(item => item.IsActive)
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Text))
        {
            var pattern = $"%{filter.Text.Trim()}%";
            query = query.Where(item =>
                EF.Functions.ILike(item.Name, pattern) ||
                EF.Functions.ILike(item.Article, pattern) ||
                EF.Functions.ILike(item.Description, pattern));
        }

        if (filter.CategoryId is not null)
            query = query.Where(item => item.CategoryId == filter.CategoryId);
        if (filter.Condition is not null)
            query = query.Where(item => item.Condition == filter.Condition);
        if (filter.MinPrice is not null)
            query = query.Where(item => item.Price >= filter.MinPrice);
        if (filter.MaxPrice is not null)
            query = query.Where(item => item.Price <= filter.MaxPrice);
        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                EF.Functions.ILike(compatibility.Make, filter.Make.Trim())));
        }
        if (!string.IsNullOrWhiteSpace(filter.Model))
        {
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                EF.Functions.ILike(compatibility.Model, filter.Model.Trim())));
        }
        if (filter.Year is not null)
        {
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                compatibility.YearFrom <= filter.Year && compatibility.YearTo >= filter.Year));
        }
        if (!string.IsNullOrWhiteSpace(filter.Engine))
        {
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                compatibility.Engine == null ||
                EF.Functions.ILike(compatibility.Engine, filter.Engine.Trim())));
        }

        // Count выполняется до Skip/Take: клиент получает количество страниц,
        // а не только размер текущей выборки.
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Article)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken) =>
        // Сущность остаётся tracked: корзина, checkout и администрирование могут
        // изменить её и сохранить через общую единицу работы.
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Product?> FindProductByArticleAsync(
        string article,
        CancellationToken cancellationToken) =>
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Article == article, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ProductArticleExistsAsync(
        string article,
        CancellationToken cancellationToken) =>
        db.Products.AnyAsync(item => item.Article == article, cancellationToken);

    /// <inheritdoc />
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken) =>
        await db.Products.AddAsync(product, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    /// <inheritdoc />
    public Task<Category?> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Category?> FindCategoryBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Slug == slug, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CategorySlugExistsAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(item => item.Slug == slug, cancellationToken);

    /// <inheritdoc />
    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken) =>
        await db.Categories.AddAsync(category, cancellationToken);
}
