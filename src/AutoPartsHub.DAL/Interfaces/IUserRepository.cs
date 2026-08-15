using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения пользователей.</summary>
public interface IUserRepository
{
    /// <summary>Находит пользователя по внутреннему идентификатору.</summary>
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Находит пользователя по идентификатору чата Telegram.</summary>
    Task<User?> FindByTelegramAsync(long chatId, CancellationToken cancellationToken);

    /// <summary>Добавляет пользователя в контекст хранения.</summary>
    Task AddAsync(User user, CancellationToken cancellationToken);
}
