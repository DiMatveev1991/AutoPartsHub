using AutoPartsHub.BLL;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.BLL.Services;

/// <summary>
/// Регистрирует пользователей Telegram и поддерживает их роли.
/// </summary>
/// <param name="users">Хранилище пользователей.</param>
/// <param name="unitOfWork">Граница сохранения изменений.</param>
/// <param name="clock">Источник текущего времени.</param>
public sealed class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IClock clock) : IUserService
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
        var user = await users.FindByTelegramAsync(telegramChatId, cancellationToken);
        if (user is null)
        {
            user = UserRules.Create(
                telegramChatId,
                displayName,
                isAdmin ? UserRole.Admin : UserRole.Customer,
                clock.UtcNow);
            await users.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (isAdmin && user.Role != UserRole.Admin)
        {
            // Список администраторов задаётся конфигурацией и применяется при следующей команде.
            UserRules.PromoteToAdmin(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return user.ToDto();
    }
}
