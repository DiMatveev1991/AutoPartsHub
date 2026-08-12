namespace AutoPartsHub.TelegramBot.Telegram;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string? BotToken { get; init; }
    public bool EnablePolling { get; init; }
    public long[] AdminChatIds { get; init; } = [];
    public bool EnableConsole { get; init; } = true;
    public long ConsoleChatId { get; init; } = 1;
}
