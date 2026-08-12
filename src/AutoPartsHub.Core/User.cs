namespace AutoPartsHub.Core;

public sealed class User
{
    private User()
    {
    }

    public User(
        long telegramChatId,
        string displayName,
        UserRole role,
        DateTimeOffset createdAt)
    {
        if (telegramChatId <= 0)
            throw new DomainException("Telegram chat id должен быть положительным.");

        Id = Guid.NewGuid();
        TelegramChatId = telegramChatId;
        DisplayName = Required(displayName, nameof(displayName), 120);
        Role = role;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public long TelegramChatId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void PromoteToAdmin() => Role = UserRole.Admin;

    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result))
            throw new DomainException($"Поле {name} обязательно.");
        if (result.Length > maxLength)
            throw new DomainException($"Поле {name} не должно превышать {maxLength} символов.");
        return result;
    }
}
