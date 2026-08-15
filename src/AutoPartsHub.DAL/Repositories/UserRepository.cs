using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует доступ к пользователям через EF Core.</summary>
internal sealed class UserRepository(AutoPartsDbContext db) : IUserRepository
{
    /// <summary>
    /// Находит пользователя по внутреннему идентификатору как отслеживаемую сущность.
    /// Отслеживание позволяет BLL изменить профиль или роль и сохранить результат через общий Unit of Work.
    /// </summary>
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <summary>
    /// Находит пользователя по уникальному идентификатору Telegram-чата как отслеживаемую сущность.
    /// Этот поиск связывает внешний Telegram API с внутренним идентификатором, не передавая детали EF Core в BLL.
    /// </summary>
    public Task<User?> FindByTelegramAsync(long chatId, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.TelegramChatId == chatId, cancellationToken);

    /// <summary>
    /// Регистрирует нового пользователя в контексте без немедленного сохранения.
    /// Unit of Work определяет момент commit и может объединить регистрацию с другими изменениями сценария.
    /// </summary>
    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);
}
