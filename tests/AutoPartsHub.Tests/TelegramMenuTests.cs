using AutoPartsHub.TelegramBot.Telegram;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет Telegram UI отдельно от сети и настоящего бота.
/// </summary>
public sealed class TelegramMenuTests
{
    /// <summary>
    /// Проверяет, что каждая reply-кнопка запускает предназначенную для неё slash-команду.
    /// Это защищает UI от незаметного рассогласования с маршрутизацией <c>BotCommandHandler</c>.
    /// </summary>
    [Theory]
    [InlineData(TelegramMenu.CatalogButton, "/catalog")]
    [InlineData(TelegramMenu.CategoriesButton, "/categories")]
    [InlineData(TelegramMenu.CartButton, "/cart")]
    [InlineData(TelegramMenu.OrdersButton, "/orders")]
    [InlineData(TelegramMenu.NotificationsButton, "/notifications")]
    [InlineData(TelegramMenu.HelpButton, "/help")]
    public void ResolveCommand_MapsMenuButton(string button, string expectedCommand) =>
        Assert.Equal(expectedCommand, TelegramMenu.ResolveCommand(button));

    /// <summary>
    /// Проверяет, что ручная команда с параметрами проходит через UI-адаптер без изменения.
    /// Благодаря этому добавление кнопок не ломает ввод VIN, артикулов и данных заказа.
    /// </summary>
    [Fact]
    public void ResolveCommand_PreservesManualCommand()
    {
        const string command = "/find фильтр масляный";

        Assert.Equal(command, TelegramMenu.ResolveCommand(command));
    }

    /// <summary>
    /// Проверяет состав и настройки постоянной клавиатуры, не отправляя запрос в Telegram API.
    /// </summary>
    [Fact]
    public void CreateMainKeyboard_ContainsAllButtons()
    {
        var keyboard = TelegramMenu.CreateMainKeyboard();
        var captions = keyboard.Keyboard
            .SelectMany(row => row)
            .Select(button => button.Text)
            .ToArray();

        Assert.Equal(6, captions.Length);
        Assert.Contains(TelegramMenu.CatalogButton, captions);
        Assert.Contains(TelegramMenu.CategoriesButton, captions);
        Assert.Contains(TelegramMenu.CartButton, captions);
        Assert.Contains(TelegramMenu.OrdersButton, captions);
        Assert.Contains(TelegramMenu.NotificationsButton, captions);
        Assert.Contains(TelegramMenu.HelpButton, captions);
        Assert.True(keyboard.ResizeKeyboard);
        Assert.True(keyboard.IsPersistent);
    }
}
