using AutoPartsHub.Core;

namespace AutoPartsHub.BLL.Contracts;

public sealed record UserDto(
    Guid Id,
    long TelegramChatId,
    string DisplayName,
    UserRole Role);
