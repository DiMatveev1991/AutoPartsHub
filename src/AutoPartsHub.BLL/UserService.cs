using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

public sealed class UserService(IAutoPartsRepository repository, IClock clock)
{
    public async Task<UserDto> GetOrCreateAsync(
        long telegramChatId,
        string displayName,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByTelegramAsync(telegramChatId, cancellationToken);
        if (user is null)
        {
            user = new User(
                telegramChatId,
                displayName,
                isAdmin ? UserRole.Admin : UserRole.Customer,
                clock.UtcNow);
            await repository.AddUserAsync(user, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
        else if (isAdmin && user.Role != UserRole.Admin)
        {
            user.PromoteToAdmin();
            await repository.SaveChangesAsync(cancellationToken);
        }

        return user.ToDto();
    }
}
