using AutoPartsHub.Core;

namespace AutoPartsHub.Tests;

public sealed class UserTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void User_UsesTelegramIdentity()
    {
        var user = new User(123456789, "Иван", UserRole.Customer, Now);

        Assert.Equal(123456789, user.TelegramChatId);
        Assert.Equal("Иван", user.DisplayName);
        Assert.Equal(UserRole.Customer, user.Role);
    }

    [Fact]
    public void User_RejectsInvalidTelegramChatId()
    {
        Assert.Throws<DomainException>(() =>
            new User(0, "Иван", UserRole.Customer, Now));
    }

    [Fact]
    public void User_CanBePromotedToAdmin()
    {
        var user = new User(123456789, "Иван", UserRole.Customer, Now);

        user.PromoteToAdmin();

        Assert.Equal(UserRole.Admin, user.Role);
    }
}
