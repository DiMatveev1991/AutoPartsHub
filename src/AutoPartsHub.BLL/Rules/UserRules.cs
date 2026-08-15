using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Создаёт и изменяет модель пользователя по правилам регистрации.
/// </summary>
internal static class UserRules
{
    /// <summary>Создаёт пользователя из уже определённой Telegram-роли.</summary>
    internal static User Create(
        long telegramChatId,
        string displayName,
        UserRole role,
        DateTimeOffset createdAt)
    {
        if (telegramChatId <= 0)
            throw new DomainException("Telegram chat id должен быть положительным.");

        return new User
        {
            Id = Guid.NewGuid(),
            TelegramChatId = telegramChatId,
            DisplayName = ValidationRules.Required(displayName, nameof(displayName), 120),
            Role = role,
            CreatedAt = createdAt
        };
    }

    /// <summary>Применяет роль администратора к существующей модели.</summary>
    internal static void PromoteToAdmin(User user) => user.Role = UserRole.Admin;
}
