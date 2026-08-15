using System.Globalization;
using System.Text;
using AutoPartsHub.BLL;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using Microsoft.Extensions.Options;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Разбирает пользовательские команды и связывает интерфейс бота с бизнес-сервисами.
/// </summary>
/// <param name="users">Сервис пользователей.</param>
/// <param name="catalog">Сервис каталога.</param>
/// <param name="carts">Сервис корзины.</param>
/// <param name="orders">Сервис заказов.</param>
/// <param name="vehicles">Сервис автомобилей.</param>
/// <param name="subscriptions">Сервис подписок и уведомлений.</param>
/// <param name="admin">Административный сервис.</param>
/// <param name="options">Настройки Telegram.</param>
public sealed class BotCommandHandler(
    IUserService users,
    ICatalogService catalog,
    ICartService carts,
    IOrderService orders,
    IVehicleService vehicles,
    ISubscriptionService subscriptions,
    IAdminService admin,
    IOptions<TelegramOptions> options)
{
    /// <summary>
    /// Выполняет одну текстовую команду от пользователя Telegram или консоли.
    /// </summary>
    public async Task<string> HandleAsync(
        long chatId,
        string displayName,
        string input,
        CancellationToken cancellationToken)
    {
        try
        {
            var isConfiguredAdmin = options.Value.AdminChatIds.Contains(chatId);
            var user = await users.GetOrCreateAsync(
                chatId,
                displayName,
                isConfiguredAdmin,
                cancellationToken);

            // Команда отделяется от аргументов один раз, после чего маршрутизируется
            // в соответствующий пользовательский сценарий.
            var (command, argument) = ParseCommand(input);
            return command switch
            {
                "/start" => $"Добро пожаловать, {user.DisplayName}!\n\n{HelpText}",
                "/help" => HelpText,
                "/catalog" => await CatalogAsync(null, cancellationToken),
                "/categories" => await CategoriesAsync(cancellationToken),
                "/find" => await CatalogAsync(Require(argument, "/find АРТИКУЛ или название"), cancellationToken),
                "/vin" => await VinAsync(argument, cancellationToken),
                "/vehicle" => await AddVehicleAsync(user.Id, argument, cancellationToken),
                "/cart" => FormatCart(await carts.GetAsync(user.Id, cancellationToken)),
                "/addcart" => await AddCartAsync(user.Id, argument, cancellationToken),
                "/remove" => await RemoveFromCartAsync(user.Id, argument, cancellationToken),
                "/checkout" => await CheckoutAsync(user.Id, argument, cancellationToken),
                "/orders" => await OrdersAsync(user.Id, cancellationToken),
                "/status" => await StatusAsync(user.Id, argument, cancellationToken),
                "/subscribe" => await SubscribeAsync(user.Id, argument, cancellationToken),
                "/notifications" => await NotificationsAsync(user.Id, cancellationToken),
                "/addcategory" => await AddCategoryAsync(user, argument, cancellationToken),
                "/addproduct" => await AddProductAsync(user, argument, cancellationToken),
                "/adminorders" => await AdminOrdersAsync(user, cancellationToken),
                "/setstatus" => await SetStatusAsync(user, argument, cancellationToken),
                _ => "Неизвестная команда. Используйте /help."
            };
        }
        // Здесь перехватываются только ожидаемые ошибки ввода и бизнес-правил.
        // Неожиданные технические ошибки поднимаются в TelegramUpdateHandler,
        // где журналируются с полным stack trace и скрываются от пользователя.
        catch (Exception exception) when (
            exception is DomainException or AppException or FormatException or ArgumentException)
        {
            return $"Ошибка: {exception.Message}";
        }
    }

    /// <summary>
    /// Возвращает первые десять товаров каталога с необязательным поисковым запросом.
    /// </summary>
    private async Task<string> CatalogAsync(string? query, CancellationToken cancellationToken)
    {
        var result = await catalog.SearchAsync(
            new CatalogFilter(Query: query, Page: 1, PageSize: 10),
            cancellationToken);
        return FormatProducts(result.Items, "Каталог пуст.");
    }

    /// <summary>
    /// Формирует текстовый список категорий.
    /// </summary>
    private async Task<string> CategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await catalog.GetCategoriesAsync(cancellationToken);
        return categories.Count == 0
            ? "Категории пока не добавлены."
            : "Категории:\n" + string.Join(
                '\n',
                categories.Select(item => $"• {item.Name} ({item.Slug})"));
    }

    /// <summary>
    /// Подбирает и форматирует товары по VIN автомобиля.
    /// </summary>
    private async Task<string> VinAsync(string argument, CancellationToken cancellationToken)
    {
        var vin = Require(argument, "/vin JT2BG22K1V0123456");
        var result = await catalog.SearchByVinAsync(vin, 1, 10, cancellationToken);
        return FormatProducts(result.Items, "Для этого автомобиля товары не найдены.");
    }

    /// <summary>
    /// Разбирает параметры и сохраняет автомобиль пользователя.
    /// </summary>
    private async Task<string> AddVehicleAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var values = Split(argument, 4, 5, "/vehicle VIN|Марка|Модель|Год|Двигатель");
        var vehicle = await vehicles.AddAsync(
            userId,
            new AddVehicleRequest(
                values[0],
                values[1],
                values[2],
                ParseInt(values[3], "Год"),
                values.Length == 5 ? values[4] : null),
            cancellationToken);
        return $"Автомобиль сохранён: {vehicle.Make} {vehicle.Model}, VIN {vehicle.Vin}.";
    }

    /// <summary>
    /// Добавляет товар в корзину по артикулу и количеству.
    /// </summary>
    private async Task<string> AddCartAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var values = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 2)
            throw new FormatException("Формат: /addcart АРТИКУЛ КОЛИЧЕСТВО");

        var product = await catalog.GetByArticleAsync(values[0], cancellationToken);
        var cart = await carts.AddAsync(
            userId,
            new AddCartItemRequest(product.Id, ParseInt(values[1], "Количество")),
            cancellationToken);
        return $"Товар добавлен.\n{FormatCart(cart)}";
    }

    /// <summary>
    /// Удаляет товар из корзины по артикулу.
    /// </summary>
    private async Task<string> RemoveFromCartAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var article = Require(argument, "/remove АРТИКУЛ");
        var product = await catalog.GetByArticleAsync(article, cancellationToken);
        var cart = await carts.RemoveAsync(userId, product.Id, cancellationToken);
        return $"Товар удалён.\n{FormatCart(cart)}";
    }

    /// <summary>
    /// Разбирает контактные данные и оформляет заказ.
    /// </summary>
    private async Task<string> CheckoutAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var values = Split(
            argument,
            3,
            5,
            "/checkout Имя|Телефон|Адрес|Courier|CashOnDelivery");
        var delivery = values.Length >= 4
            ? ParseEnum<DeliveryMethod>(values[3], "Способ доставки")
            : DeliveryMethod.Courier;
        var payment = values.Length == 5
            ? ParseEnum<PaymentMethod>(values[4], "Способ оплаты")
            : PaymentMethod.CashOnDelivery;

        var order = await orders.CheckoutAsync(
            userId,
            new CheckoutRequest(values[0], values[1], values[2], delivery, payment),
            cancellationToken);
        return $"Заказ {order.OrderNumber} оформлен. Сумма {order.Total:F2} ₽, статус {order.Status}.";
    }

    /// <summary>
    /// Формирует краткую историю заказов пользователя.
    /// </summary>
    private async Task<string> OrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await orders.GetMineAsync(userId, cancellationToken);
        return items.Count == 0
            ? "У вас пока нет заказов."
            : "Ваши заказы:\n" + string.Join(
                '\n',
                items.Select(item =>
                    $"• {item.OrderNumber}: {item.Status}, {item.Total:F2} ₽"));
    }

    /// <summary>
    /// Возвращает состояние заказа по его номеру.
    /// </summary>
    private async Task<string> StatusAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var number = Require(argument, "/status НОМЕР_ЗАКАЗА");
        var order = await orders.FindByNumberAsync(userId, number, cancellationToken);
        return $"Заказ {order.OrderNumber}: {order.Status}. Сумма {order.Total:F2} ₽.";
    }

    /// <summary>
    /// Создаёт подписку на наличие товара или снижение цены.
    /// </summary>
    private async Task<string> SubscribeAsync(
        Guid userId,
        string argument,
        CancellationToken cancellationToken)
    {
        var values = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length is < 1 or > 2)
            throw new FormatException("Формат: /subscribe АРТИКУЛ [ЦЕЛЕВАЯ_ЦЕНА]");

        var product = await catalog.GetByArticleAsync(values[0], cancellationToken);
        var type = values.Length == 1 ? SubscriptionType.BackInStock : SubscriptionType.PriceDrop;
        decimal? price = values.Length == 2 ? ParseDecimal(values[1], "Цена") : null;
        await subscriptions.SubscribeAsync(
            userId,
            new SubscribeRequest(product.Id, type, price),
            cancellationToken);
        return type == SubscriptionType.BackInStock
            ? $"Подписка на наличие товара {product.Article} создана."
            : $"Сообщим, когда цена {product.Article} станет не выше {price:F2} ₽.";
    }

    /// <summary>
    /// Возвращает последние уведомления пользователя.
    /// </summary>
    private async Task<string> NotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var items = await subscriptions.GetNotificationsAsync(userId, cancellationToken);
        return items.Count == 0
            ? "Уведомлений пока нет."
            : "Уведомления:\n" + string.Join(
                '\n',
                items.Take(10).Select(item => $"• {item.Text} ({item.Status})"));
    }

    /// <summary>
    /// Создаёт категорию после проверки роли администратора.
    /// </summary>
    private async Task<string> AddCategoryAsync(
        UserDto user,
        string argument,
        CancellationToken cancellationToken)
    {
        RequireAdmin(user);
        var values = Split(argument, 2, 2, "/addcategory Название|slug");
        var category = await admin.CreateCategoryAsync(
            new CreateCategoryRequest(values[0], values[1]),
            cancellationToken);
        return $"Категория {category.Name} создана.";
    }

    /// <summary>
    /// Создаёт товар после проверки роли администратора.
    /// </summary>
    private async Task<string> AddProductAsync(
        UserDto user,
        string argument,
        CancellationToken cancellationToken)
    {
        RequireAdmin(user);
        var values = Split(
            argument,
            7,
            7,
            "/addproduct slug|артикул|название|описание|New|цена|остаток");
        var categories = await catalog.GetCategoriesAsync(cancellationToken);
        var category = categories.SingleOrDefault(item =>
            string.Equals(item.Slug, values[0], StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("Категория с таким slug не найдена.");

        var product = await admin.CreateProductAsync(
            new CreateProductRequest(
                category.Id,
                values[1],
                values[2],
                values[3],
                ParseEnum<ProductCondition>(values[4], "Состояние"),
                ParseDecimal(values[5], "Цена"),
                ParseInt(values[6], "Остаток"),
                []),
            cancellationToken);
        return $"Товар {product.Article} создан.";
    }

    /// <summary>
    /// Возвращает администратору последние заказы.
    /// </summary>
    private async Task<string> AdminOrdersAsync(
        UserDto user,
        CancellationToken cancellationToken)
    {
        RequireAdmin(user);
        var items = await admin.GetOrdersAsync(cancellationToken);
        return items.Count == 0
            ? "Заказов пока нет."
            : "Все заказы:\n" + string.Join(
                '\n',
                items.Take(20).Select(item =>
                    $"• {item.OrderNumber}: {item.Status}, {item.Total:F2} ₽"));
    }

    /// <summary>
    /// Изменяет статус заказа после проверки роли администратора.
    /// </summary>
    private async Task<string> SetStatusAsync(
        UserDto user,
        string argument,
        CancellationToken cancellationToken)
    {
        RequireAdmin(user);
        var values = Split(argument, 2, 2, "/setstatus НОМЕР|Shipped");
        var items = await admin.GetOrdersAsync(cancellationToken);
        var order = items.SingleOrDefault(item =>
            string.Equals(item.OrderNumber, values[0], StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("Заказ не найден.");
        var updated = await admin.ChangeOrderStatusAsync(
            order.Id,
            new ChangeOrderStatusRequest(ParseEnum<OrderStatus>(values[1], "Статус")),
            cancellationToken);
        return $"Статус заказа {updated.OrderNumber} изменён на {updated.Status}.";
    }

    /// <summary>
    /// Форматирует коллекцию товаров для текстового интерфейса.
    /// </summary>
    private static string FormatProducts(
        IReadOnlyCollection<ProductDto> products,
        string emptyMessage)
    {
        if (products.Count == 0)
            return emptyMessage;

        var text = new StringBuilder("Товары:");
        foreach (var item in products)
        {
            text.AppendLine()
                .Append("• ")
                .Append(item.Article)
                .Append(" — ")
                .Append(item.Name)
                .Append(", ")
                .Append(item.Price.ToString("F2", CultureInfo.CurrentCulture))
                .Append(" ₽, остаток ")
                .Append(item.Stock);
        }
        return text.ToString();
    }

    /// <summary>
    /// Форматирует корзину с позициями и общей стоимостью.
    /// </summary>
    private static string FormatCart(CartDto cart)
    {
        if (cart.Items.Count == 0)
            return "Корзина пуста.";

        var lines = cart.Items.Select(item =>
            $"• {item.Article} — {item.Quantity} × {item.UnitPrice:F2} = {item.LineTotal:F2} ₽");
        return "Корзина:\n" + string.Join('\n', lines) + $"\nИтого: {cart.Total:F2} ₽";
    }

    /// <summary>
    /// Выделяет имя команды и оставшуюся строку аргументов.
    /// </summary>
    private static (string Command, string Argument) ParseCommand(string input)
    {
        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return (string.Empty, string.Empty);
        // Telegram может прислать команду вида /catalog@BotName в групповом чате.
        var command = parts[0].Split('@', 2)[0].ToLowerInvariant();
        return (command, parts.Length == 2 ? parts[1].Trim() : string.Empty);
    }

    /// <summary>
    /// Возвращает обязательный аргумент или сообщает ожидаемый формат команды.
    /// </summary>
    private static string Require(string value, string example)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException($"Формат: {example}");
        return value.Trim();
    }

    /// <summary>
    /// Разделяет аргументы по вертикальной черте и проверяет их количество.
    /// </summary>
    private static string[] Split(string value, int min, int max, string example)
    {
        var result = value.Split('|', StringSplitOptions.TrimEntries);
        if (result.Length < min || result.Length > max || result.Any(string.IsNullOrWhiteSpace))
            throw new FormatException($"Формат: {example}");
        return result;
    }

    /// <summary>
    /// Преобразует строку в целое число или создаёт понятную ошибку формата.
    /// </summary>
    private static int ParseInt(string value, string field) =>
        int.TryParse(value, out var result)
            ? result
            : throw new FormatException($"{field}: требуется целое число.");

    /// <summary>
    /// Преобразует цену с учётом текущей и инвариантной культуры.
    /// </summary>
    private static decimal ParseDecimal(string value, string field)
    {
        // Поддерживаются и локальный десятичный разделитель, и точка из примеров README.
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            return result;
        throw new FormatException($"{field}: требуется число.");
    }

    /// <summary>
    /// Преобразует строку в определённое значение перечисления без учёта регистра.
    /// </summary>
    private static T ParseEnum<T>(string value, string field)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var result) && Enum.IsDefined(result)
            ? result
            : throw new FormatException(
                $"{field}: допустимы {string.Join(", ", Enum.GetNames<T>())}.");

    /// <summary>
    /// Проверяет право пользователя выполнять административную команду.
    /// </summary>
    private static void RequireAdmin(UserDto user)
    {
        // Роль загружена из БД сервисом пользователей и не передаётся в тексте
        // команды, поэтому пользователь не может назначить её самостоятельно.
        if (user.Role != UserRole.Admin)
            throw new AppException("Команда доступна только администратору.");
    }

    /// <summary>Текст справки по пользовательским и административным командам.</summary>
    private const string HelpText =
        "AutoParts Hub\n" +
        "/catalog — каталог\n" +
        "/categories — категории\n" +
        "/find <текст> — поиск по названию или артикулу\n" +
        "/vehicle VIN|Марка|Модель|Год|Двигатель — сохранить автомобиль\n" +
        "/vin <VIN> — подобрать совместимые детали\n" +
        "/addcart <артикул> <количество> — добавить в корзину\n" +
        "/cart — показать корзину\n" +
        "/remove <артикул> — удалить из корзины\n" +
        "/checkout Имя|Телефон|Адрес|Courier|CashOnDelivery — оформить заказ\n" +
        "/orders — история заказов\n" +
        "/status <номер> — статус заказа\n" +
        "/subscribe <артикул> [цена] — подписка на наличие или снижение цены\n" +
        "/notifications — уведомления\n" +
        "Администратор: /addcategory, /addproduct, /adminorders, /setstatus.";
}
