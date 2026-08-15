using AutoPartsHub.BLL;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.Models;

namespace AutoPartsHub.Tests;

/// <summary>Проверяет правила BLL для регистрации и ролей пользователя.</summary>
public sealed class UserTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Проверяет сохранение Telegram-идентичности пользователя.</summary>
    [Fact]
    public void Create_UsesTelegramIdentity()
    {
        var user = UserRules.Create(123456789, "Иван", UserRole.Customer, Now);

        Assert.Equal(123456789, user.TelegramChatId);
        Assert.Equal("Иван", user.DisplayName);
        Assert.Equal(UserRole.Customer, user.Role);
    }

    /// <summary>Проверяет отклонение неположительного идентификатора чата.</summary>
    [Fact]
    public void Create_RejectsInvalidTelegramChatId()
    {
        Assert.Throws<DomainException>(() =>
            UserRules.Create(0, "Иван", UserRole.Customer, Now));
    }

    /// <summary>Проверяет повышение пользователя до администратора.</summary>
    [Fact]
    public void PromoteToAdmin_ChangesRole()
    {
        var user = UserRules.Create(123456789, "Иван", UserRole.Customer, Now);

        UserRules.PromoteToAdmin(user);

        Assert.Equal(UserRole.Admin, user.Role);
    }
}
