namespace AutoPartsHub.TelegramBot.Telegram;

/// <summary>
/// Содержит настройки Telegram-бота и консольного режима.
/// </summary>
public sealed class TelegramOptions
{
    /// <summary>Название секции конфигурации Telegram.</summary>
    public const string SectionName = "Telegram";

    /// <summary>Получает токен Telegram-бота.</summary>
    public string? BotToken { get; init; }

    /// <summary>Указывает, следует ли получать обновления через long polling.</summary>
    public bool EnablePolling { get; init; }

    /// <summary>Получает идентификаторы чатов администраторов.</summary>
    public long[] AdminChatIds { get; init; } = [];

    /// <summary>Указывает, следует ли запускать консольный интерфейс.</summary>
    public bool EnableConsole { get; init; } = true;

    /// <summary>Получает виртуальный идентификатор пользователя консольного режима.</summary>
    public long ConsoleChatId { get; init; } = 1;
}
