namespace AutoPartsHub.Core;

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

public interface IAutoPartsRepository
{
    Task<User?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindUserByTelegramAsync(long chatId, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchProductsAsync(
        ProductSearchQuery query,
        CancellationToken cancellationToken);
    Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> FindProductByArticleAsync(string article, CancellationToken cancellationToken);
    Task<bool> ProductArticleExistsAsync(string article, CancellationToken cancellationToken);
    Task AddProductAsync(Product product, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<Category?> FindCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<Category?> FindCategoryBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> CategorySlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken);

    Task<Cart?> FindCartAsync(Guid userId, CancellationToken cancellationToken);
    Task AddCartAsync(Cart cart, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Order>> GetOrdersAsync(Guid? userId, CancellationToken cancellationToken);
    Task<Order?> FindOrderAsync(Guid id, CancellationToken cancellationToken);
    Task AddOrderAsync(Order order, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Vehicle>> GetVehiclesAsync(Guid userId, CancellationToken cancellationToken);
    Task<Vehicle?> FindVehicleByVinAsync(string vin, CancellationToken cancellationToken);
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken);

    Task<bool> ActiveSubscriptionExistsAsync(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        CancellationToken cancellationToken);
    Task AddSubscriptionAsync(ProductSubscription subscription, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredSubscriptionsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Notification>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Notification>> GetPendingNotificationsAsync(
        CancellationToken cancellationToken);
    Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);
}
