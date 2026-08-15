using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoPartsHub.DAL.Persistence;

/// <summary>
/// Реализует доступ к данным AutoParts Hub через Entity Framework Core и PostgreSQL.
/// </summary>
/// <param name="db">Контекст базы данных.</param>
public sealed class AutoPartsRepository(AutoPartsDbContext db) : IAutoPartsRepository
{
    /// <summary>Находит пользователя по внутреннему идентификатору.</summary>
    public Task<User?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>Находит пользователя по идентификатору чата Telegram.</summary>
    public Task<User?> FindUserByTelegramAsync(long chatId, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.TelegramChatId == chatId, cancellationToken);

    /// <summary>Добавляет пользователя в контекст базы данных.</summary>
    public async Task AddUserAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);

    /// <summary>
    /// Формирует запрос к каталогу, применяет фильтры и возвращает страницу товаров.
    /// </summary>
    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchProductsAsync(
        ProductSearchQuery filter,
        CancellationToken cancellationToken)
    {
        // Каталог используется только для чтения, поэтому AsNoTracking уменьшает
        // расходы Change Tracker. AsSplitQuery предотвращает декартово умножение
        // строк при загрузке категории и коллекции совместимостей одним запросом.
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

        // Количество вычисляется до Skip/Take, чтобы корректно построить пагинацию.
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Article)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    /// <summary>Находит товар с категорией и совместимостями по идентификатору.</summary>
    public Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken) =>
        // Товар остаётся tracked: этот метод используют корзина, checkout и
        // администрирование, где доменная сущность может быть изменена и сохранена.
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>Находит товар с категорией и совместимостями по артикулу.</summary>
    public Task<Product?> FindProductByArticleAsync(
        string article,
        CancellationToken cancellationToken) =>
        db.Products
            .Include(item => item.Category)
            .Include(item => item.Compatibilities)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Article == article, cancellationToken);

    /// <summary>Проверяет существование товара с указанным артикулом.</summary>
    public Task<bool> ProductArticleExistsAsync(string article, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(item => item.Article == article, cancellationToken);

    /// <summary>Добавляет товар в контекст базы данных.</summary>
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken) =>
        await db.Products.AddAsync(product, cancellationToken);

    /// <summary>Возвращает категории в алфавитном порядке.</summary>
    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    /// <summary>Находит категорию по идентификатору.</summary>
    public Task<Category?> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>Находит категорию по slug.</summary>
    public Task<Category?> FindCategoryBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.Categories.SingleOrDefaultAsync(item => item.Slug == slug, cancellationToken);

    /// <summary>Проверяет существование категории с указанным slug.</summary>
    public Task<bool> CategorySlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(item => item.Slug == slug, cancellationToken);

    /// <summary>Добавляет категорию в контекст базы данных.</summary>
    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken) =>
        await db.Categories.AddAsync(category, cancellationToken);

    /// <summary>Находит корзину пользователя вместе с позициями и товарами.</summary>
    public Task<Cart?> FindCartAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Carts
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    /// <summary>Добавляет корзину в контекст базы данных.</summary>
    public async Task AddCartAsync(Cart cart, CancellationToken cancellationToken) =>
        await db.Carts.AddAsync(cart, cancellationToken);

    /// <summary>Возвращает заказы всех пользователей или выбранного пользователя.</summary>
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

    /// <summary>Находит заказ вместе с позициями по идентификатору.</summary>
    public Task<Order?> FindOrderAsync(Guid id, CancellationToken cancellationToken) =>
        db.Orders
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>Добавляет заказ в контекст базы данных.</summary>
    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken) =>
        await db.Orders.AddAsync(order, cancellationToken);

    /// <summary>Возвращает автомобили пользователя в порядке марки и модели.</summary>
    public async Task<IReadOnlyCollection<Vehicle>> GetVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Vehicles
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Make)
            .ThenBy(item => item.Model)
            .ToArrayAsync(cancellationToken);

    /// <summary>Находит автомобиль по нормализованному VIN.</summary>
    public Task<Vehicle?> FindVehicleByVinAsync(string vin, CancellationToken cancellationToken) =>
        db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Vin == vin, cancellationToken);

    /// <summary>Добавляет автомобиль в контекст базы данных.</summary>
    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
        await db.Vehicles.AddAsync(vehicle, cancellationToken);

    /// <summary>Проверяет наличие активной подписки пользователя на товар.</summary>
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

    /// <summary>Добавляет товарную подписку в контекст базы данных.</summary>
    public async Task AddSubscriptionAsync(
        ProductSubscription subscription,
        CancellationToken cancellationToken) =>
        await db.ProductSubscriptions.AddAsync(subscription, cancellationToken);

    /// <summary>Возвращает активные подписки, условия которых уже выполнены.</summary>
    public async Task<IReadOnlyCollection<ProductSubscription>> GetTriggeredSubscriptionsAsync(
        CancellationToken cancellationToken) =>
        // Условие повторяет ProductSubscription.IsTriggeredBy, потому что доменный
        // метод нельзя перевести в SQL. Фильтрация в БД не загружает все подписки в память.
        await db.ProductSubscriptions
            .Include(item => item.Product)
            .Where(item => item.IsActive &&
                (item.Type == SubscriptionType.BackInStock && item.Product!.Stock > 0 ||
                 item.Type == SubscriptionType.PriceDrop && item.Product!.Price <= item.TargetPrice))
            .ToArrayAsync(cancellationToken);

    /// <summary>Возвращает уведомления пользователя от новых к старым.</summary>
    public async Task<IReadOnlyCollection<Notification>> GetNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToArrayAsync(cancellationToken);

    /// <summary>Возвращает до ста ожидающих уведомлений для пакетной отправки.</summary>
    public async Task<IReadOnlyCollection<Notification>> GetPendingNotificationsAsync(
        CancellationToken cancellationToken) =>
        await db.Notifications
            .Include(item => item.User)
            .Where(item => item.Status == NotificationStatus.Pending)
            .OrderBy(item => item.CreatedAt)
            // Ограничение размера не даёт одному циклу фонового worker занять память
            // и задержать приложение при большой очереди уведомлений.
            .Take(100)
            .ToArrayAsync(cancellationToken);

    /// <summary>Добавляет уведомление в контекст базы данных.</summary>
    public async Task AddNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken) =>
        await db.Notifications.AddAsync(notification, cancellationToken);

    /// <summary>
    /// Сохраняет изменения и преобразует технические ошибки базы данных в понятные сообщения.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Пользователь получает прикладное сообщение вместо деталей EF Core.
            throw new InvalidOperationException(
                "Товар был изменён другим пользователем. Обновите данные и повторите операцию.",
                exception);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Уникальные индексы остаются последней защитой от гонки между двумя
            // предварительными проверками существования записи.
            throw new InvalidOperationException(
                "Запись с такими уникальными данными уже существует.",
                exception);
        }
    }

    /// <summary>
    /// Выполняет переданное действие в транзакции с явным подтверждением или откатом.
    /// </summary>
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
            // Явный rollback подчёркивает границу единицы работы; исходное
            // исключение пробрасывается выше без подмены причины.
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
