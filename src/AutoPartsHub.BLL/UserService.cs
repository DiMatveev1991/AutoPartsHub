using AutoPartsHub.DTOs;
using AutoPartsHub.Models;

namespace AutoPartsHub.BLL;

/// <summary>
/// Регистрирует пользователей Telegram и поддерживает их роли.
/// </summary>
/// <param name="repository">Хранилище данных приложения.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class UserService(IAutoPartsRepository repository, IClock clock)
{
    /// <summary>
    /// Возвращает существующего пользователя или регистрирует его при первом обращении.
    /// </summary>
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
            // Список администраторов задаётся конфигурацией и применяется при следующей команде.
            user.PromoteToAdmin();
            await repository.SaveChangesAsync(cancellationToken);
        }

        return user.ToDto();
    }
}
