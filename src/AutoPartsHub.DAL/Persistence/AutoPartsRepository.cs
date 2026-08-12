using AutoPartsHub.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoPartsHub.DAL.Persistence;

public sealed class AutoPartsRepository(AutoPartsDbContext db) : IAutoPartsRepository
{
    public Task<User?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<User?> FindUserByTelegramAsync(long chatId, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.TelegramChatId == chatId, cancellationToken);

    public async Task AddUserAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);

    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchProductsAsync(
        ProductSearchQuery filter,
        CancellationToken cancellationToken)
    {
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
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                EF.Functions.ILike(compatibility.Make, filter.Make.Trim())));
        if (!string.IsNullOrWhiteSpace(filter.Model))
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                EF.Functions.ILike(compatibility.Model, filter.Model.Trim())));
        if (filter.Year is not null)
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                compatibility.YearFrom <= filter.Year && compatibility.YearTo >= filter.Year));
        if (!string.IsNullOrWhiteSpace(filter.Engine))
            query = query.Where(item => item.Compatibilities.Any(compatibility =>
                compatibility.Engine == null ||
                EF.Functions.ILike(compatibility.Engine, filter.Engine.Trim())));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Article)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken) =>
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<Product?> FindProductByArticleAsync(
        string article,
        CancellationToken cancellationToken) =>
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Article == article, cancellationToken);

    public Task<bool> ProductArticleExistsAsync(string article, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(item => item.Article == article, cancellationToken);

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken) =>
        await db.Products.AddAsync(product, cancellationToken);

    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    public Task<Category?> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<Category?> FindCategoryBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Slug == slug, cancellationToken);

    public Task<bool> CategorySlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(item => item.Slug == slug, cancellationToken);

    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken) =>
        await db.Categories.AddAsync(category, cancellationToken);

    public Task<Cart?> FindCartAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Carts
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    public async Task AddCartAsync(Cart cart, CancellationToken cancellationToken) =>
        await db.Carts.AddAsync(cart, cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetOrdersAsync(
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var query = db.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .AsSplitQuery()
            .AsQueryable();
        if (userId is not null)
            query = query.Where(item => item.UserId == userId);

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Order?> FindOrderAsync(Guid id, CancellationToken cancellationToken) =>
        db.Orders
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken) =>
        await db.Orders.AddAsync(order, cancellationToken);

    public async Task<IReadOnlyCollection<Vehicle>> GetVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Vehicles
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Make)
            .ThenBy(item => item.Model)
            .ToArrayAsync(cancellationToken);

    public Task<Vehicle?> FindVehicleByVinAsync(string vin, CancellationToken cancellationToken) =>
        db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Vin == vin, cancellationToken);

    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
        await db.Vehicles.AddAsync(vehicle, cancellationToken);

    public Task<bool> ActiveSubscriptionExistsAsync(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        CancellationToken cancellationToken) =>
        db.ProductSubscriptions.AnyAsync(
            item => item.UserId == userId &&
                    item.ProductId == productId &&
                    item.Type == type &&
                    item.IsActive,
            cancellationToken);

    public async Task AddSubscriptionAsync(
        ProductSubscription subscription,
        CancellationToken cancellationToken) =>
        await db.ProductSubscriptions.AddAsync(subscription, cancellationToken);

    public async Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredSubscriptionsAsync(
        CancellationToken cancellationToken) =>
        await db.ProductSubscriptions
            .Include(item => item.Product)
            .Where(item => item.IsActive &&
                (item.Type == SubscriptionType.BackInStock && item.Product!.Stock > 0 ||
                 item.Type == SubscriptionType.PriceDrop && item.Product!.Price <= item.TargetPrice))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Notification>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Notification>> GetPendingNotificationsAsync(
        CancellationToken cancellationToken) =>
        await db.Notifications
            .Include(item => item.User)
            .Where(item => item.Status == NotificationStatus.Pending)
            .OrderBy(item => item.CreatedAt)
            .Take(100)
            .ToArrayAsync(cancellationToken);

    public async Task AddNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken) =>
        await db.Notifications.AddAsync(notification, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "Товар был изменён другим пользователем. Обновите данные и повторите операцию.",
                exception);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new InvalidOperationException(
                "Запись с такими уникальными данными уже существует.",
                exception);
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
