using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает сценарии регистрации и получения пользователя.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Возвращает пользователя Telegram или создаёт его при первом обращении.
    /// </summary>
    Task<UserDto> GetOrCreateAsync(
        long telegramChatId,
        string displayName,
        bool isAdmin,
        CancellationToken cancellationToken);
}
