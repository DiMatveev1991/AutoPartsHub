namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Содержит настройки HTTP-прокси для обращений к Telegram Bot API.
/// </summary>
public sealed class ProxyOptions
{
    /// <summary>Название секции конфигурации прокси.</summary>
    public const string SectionName = "Proxy";

    /// <summary>Указывает, следует ли направлять Telegram-запросы через прокси.</summary>
    public bool UseProxy { get; init; }

    /// <summary>Получает абсолютный HTTP- или HTTPS-адрес прокси.</summary>
    public string? Url { get; init; }

    /// <summary>Получает имя пользователя прокси.</summary>
    public string? Username { get; init; }

    /// <summary>Получает пароль прокси.</summary>
    public string? Password { get; init; }
}
