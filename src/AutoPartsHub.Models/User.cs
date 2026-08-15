namespace AutoPartsHub.Models;

/// <summary>
/// Представляет пользователя AutoParts Hub, связанного с учётной записью Telegram.
/// </summary>
/// <remarks>
/// Связи: один пользователь имеет не более одной корзины и может иметь много
/// автомобилей, заказов, товарных подписок и уведомлений. Обратные коллекции
/// намеренно не объявлены и настроены в DAL через Fluent API.
/// </remarks>
public sealed class User
{
    /// <summary>
    /// Создаёт экземпляр пользователя для восстановления Entity Framework Core.
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Создаёт нового пользователя и проверяет обязательные данные.
    /// </summary>
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

    /// <summary>Получает уникальный идентификатор пользователя.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает идентификатор чата пользователя в Telegram.</summary>
    public long TelegramChatId { get; private set; }

    /// <summary>Получает отображаемое имя пользователя.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Получает роль пользователя.</summary>
    public UserRole Role { get; private set; }

    /// <summary>Получает дату и время регистрации пользователя.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Назначает пользователю роль администратора.
    /// </summary>
    public void PromoteToAdmin() => Role = UserRole.Admin;

    /// <summary>
    /// Проверяет обязательную строку, удаляет крайние пробелы и контролирует длину.
    /// </summary>
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
