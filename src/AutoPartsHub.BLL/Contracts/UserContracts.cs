using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

/// <summary>
/// Представляет данные пользователя, передаваемые в слой представления.
/// </summary>
/// <param name="Id">Идентификатор пользователя.</param>
/// <param name="TelegramChatId">Идентификатор чата Telegram.</param>
/// <param name="DisplayName">Отображаемое имя.</param>
/// <param name="Role">Роль пользователя.</param>
public sealed record UserDto(
    Guid Id,
    long TelegramChatId,
    string DisplayName,
    UserRole Role);
