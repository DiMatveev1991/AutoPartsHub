using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует доступ к пользователям через EF Core.</summary>
internal sealed class UserRepository(AutoPartsDbContext db) : IUserRepository
{
    /// <inheritdoc />
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<User?> FindByTelegramAsync(long chatId, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(item => item.TelegramChatId == chatId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await db.Users.AddAsync(user, cancellationToken);
}
