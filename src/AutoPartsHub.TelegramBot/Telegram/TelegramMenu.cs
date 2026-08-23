using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Описывает элементы пользовательского интерфейса Telegram и переводит нажатия
/// кнопок в канонические команды, которые уже понимает общий обработчик бота.
/// </summary>
/// <remarks>
/// Класс находится в presentation-проекте: BLL и общий <see cref="BotCommandHandler"/>
/// не должны зависеть от подписей кнопок, эмодзи и типов Telegram Bot API.
/// </remarks>
internal static class TelegramMenu
{
    /// <summary>Подпись кнопки открытия каталога.</summary>
    internal const string CatalogButton = "🔎 Каталог";

    /// <summary>Подпись кнопки просмотра категорий.</summary>
    internal const string CategoriesButton = "📂 Категории";

    /// <summary>Подпись кнопки открытия корзины.</summary>
    internal const string CartButton = "🛒 Корзина";

    /// <summary>Подпись кнопки просмотра заказов пользователя.</summary>
    internal const string OrdersButton = "📦 Мои заказы";

    /// <summary>Подпись кнопки просмотра уведомлений.</summary>
    internal const string NotificationsButton = "🔔 Уведомления";

    /// <summary>Подпись кнопки открытия справки.</summary>
    internal const string HelpButton = "❓ Помощь";

    /// <summary>
    /// Создаёт постоянную клавиатуру для операций, которым не нужны дополнительные параметры.
    /// Команды с VIN, артикулом или данными доставки остаются текстовыми, потому что одна кнопка
    /// не может безопасно собрать и проверить все обязательные значения.
    /// </summary>
    internal static ReplyKeyboardMarkup CreateMainKeyboard() => new(
        new KeyboardButton[][]
        {
            [CatalogButton, CategoriesButton],
            [CartButton, OrdersButton],
            [NotificationsButton, HelpButton]
        })
    {
        ResizeKeyboard = true,
        IsPersistent = true,
        InputFieldPlaceholder = "Выберите действие или введите команду"
    };

    /// <summary>
    /// Возвращает команды, отображаемые Telegram в системном меню «/».
    /// Список содержит все пользовательские сценарии, чтобы функции были доступны
    /// даже в клиентах, где постоянная reply-клавиатура скрыта.
    /// </summary>
    internal static IReadOnlyCollection<BotCommand> CreateBotCommands() =>
    [
        new() { Command = "start", Description = "Открыть главное меню" },
        new() { Command = "catalog", Description = "Показать каталог" },
        new() { Command = "categories", Description = "Показать категории" },
        new() { Command = "find", Description = "Найти деталь" },
        new() { Command = "vin", Description = "Подобрать детали по VIN" },
        new() { Command = "vehicle", Description = "Сохранить автомобиль" },
        new() { Command = "cart", Description = "Открыть корзину" },
        new() { Command = "addcart", Description = "Добавить деталь в корзину" },
        new() { Command = "remove", Description = "Удалить деталь из корзины" },
        new() { Command = "checkout", Description = "Оформить заказ" },
        new() { Command = "orders", Description = "Показать мои заказы" },
        new() { Command = "status", Description = "Проверить статус заказа" },
        new() { Command = "subscribe", Description = "Подписаться на изменение товара" },
        new() { Command = "notifications", Description = "Показать уведомления" },
        new() { Command = "addcompatibility", Description = "Назначить совместимость товару" },
        new() { Command = "updateproduct", Description = "Изменить цену и остаток товара" },
        new() { Command = "deactivateproduct", Description = "Скрыть товар из каталога" },
        new() { Command = "activateproduct", Description = "Восстановить товар в каталоге" },
        new() { Command = "help", Description = "Показать справку" }
    ];

    /// <summary>
    /// Преобразует текст reply-кнопки в существующую slash-команду.
    /// Незнакомый текст возвращается без изменений, поэтому ручные команды и их
    /// аргументы продолжают обрабатываться прежним маршрутом.
    /// </summary>
    internal static string ResolveCommand(string input) => input.Trim() switch
    {
        CatalogButton => "/catalog",
        CategoriesButton => "/categories",
        CartButton => "/cart",
        OrdersButton => "/orders",
        NotificationsButton => "/notifications",
        HelpButton => "/help",
        _ => input
    };
}
