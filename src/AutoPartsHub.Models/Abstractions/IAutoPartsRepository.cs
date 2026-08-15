namespace AutoPartsHub.Models;

/// <summary>
/// Описывает параметры поиска и постраничного вывода товаров.
/// </summary>
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

/// <summary>
/// Определяет единый интерфейс доступа к данным AutoParts Hub.
/// </summary>
public interface IAutoPartsRepository
{
    /// <summary>Находит пользователя по внутреннему идентификатору.</summary>
    Task<User?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Находит пользователя по идентификатору чата Telegram.</summary>
    Task<User?> FindUserByTelegramAsync(long chatId, CancellationToken cancellationToken);

    /// <summary>Добавляет пользователя в контекст хранения.</summary>
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>Возвращает страницу товаров и общее количество результатов.</summary>
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchProductsAsync(
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

    /// <summary>Находит корзину пользователя вместе с товарными позициями.</summary>
    Task<Cart?> FindCartAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Добавляет корзину в контекст хранения.</summary>
    Task AddCartAsync(Cart cart, CancellationToken cancellationToken);

    /// <summary>Возвращает заказы всех пользователей или одного пользователя.</summary>
    Task<IReadOnlyCollection<Order>> GetOrdersAsync(Guid? userId, CancellationToken cancellationToken);

    /// <summary>Находит заказ по идентификатору.</summary>
    Task<Order?> FindOrderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Добавляет заказ в контекст хранения.</summary>
    Task AddOrderAsync(Order order, CancellationToken cancellationToken);

    /// <summary>Возвращает автомобили пользователя.</summary>
    Task<IReadOnlyCollection<Vehicle>> GetVehiclesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Находит автомобиль по нормализованному VIN.</summary>
    Task<Vehicle?> FindVehicleByVinAsync(string vin, CancellationToken cancellationToken);

    /// <summary>Добавляет автомобиль в контекст хранения.</summary>
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken);

    /// <summary>Проверяет наличие активной подписки заданного типа.</summary>
    Task<bool> ActiveSubscriptionExistsAsync(
        Guid userId,
        Guid productId,
        SubscriptionType type,
        CancellationToken cancellationToken);
    /// <summary>Добавляет товарную подписку в контекст хранения.</summary>
    Task AddSubscriptionAsync(ProductSubscription subscription, CancellationToken cancellationToken);

    /// <summary>Возвращает активные подписки, условия которых уже выполнены.</summary>
    Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredSubscriptionsAsync(
        CancellationToken cancellationToken);

    /// <summary>Возвращает историю уведомлений пользователя.</summary>
    Task<IReadOnlyCollection<Notification>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Возвращает ожидающие отправки уведомления.</summary>
    Task<IReadOnlyCollection<Notification>> GetPendingNotificationsAsync(
        CancellationToken cancellationToken);

    /// <summary>Добавляет уведомление в контекст хранения.</summary>
    Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken);

    /// <summary>Сохраняет все накопленные изменения.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Выполняет действие в транзакции базы данных.</summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);
}
