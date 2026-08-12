using AutoPartsHub.Core;

namespace AutoPartsHub.Tests;

/// <summary>
/// Проверяет правила регистрации и ролей пользователя.
/// </summary>
public sealed class UserTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет сохранение Telegram-идентичности пользователя.</summary>
    [Fact]
    public void User_UsesTelegramIdentity()
    {
        var user = new User(123456789, "Иван", UserRole.Customer, Now);

        Assert.Equal(123456789, user.TelegramChatId);
        Assert.Equal("Иван", user.DisplayName);
        Assert.Equal(UserRole.Customer, user.Role);
    }

    /// <summary>Проверяет отклонение неположительного идентификатора чата.</summary>
    [Fact]
    public void User_RejectsInvalidTelegramChatId()
    {
        Assert.Throws<DomainException>(() =>
            new User(0, "Иван", UserRole.Customer, Now));
    }

    /// <summary>Проверяет повышение пользователя до администратора.</summary>
    [Fact]
    public void User_CanBePromotedToAdmin()
    {
        var user = new User(123456789, "Иван", UserRole.Customer, Now);

        user.PromoteToAdmin();

        Assert.Equal(UserRole.Admin, user.Role);
    }
}
