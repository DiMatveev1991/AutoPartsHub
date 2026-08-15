using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует общую границу сохранения для scoped DbContext.</summary>
internal sealed class UnitOfWork(AutoPartsDbContext db) : IUnitOfWork
{
    /// <summary>
    /// Сохраняет все накопленные изменения scoped DbContext одной операцией и переводит ошибки инфраструктуры
    /// в исключения DAL, чтобы BLL не зависел от типов EF Core и конкретной СУБД.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Технические детали EF не просачиваются в пользовательский ответ.
            throw new InvalidOperationException(
                "Товар был изменён другим пользователем. Обновите данные и повторите операцию.",
                exception);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Индекс остаётся последней защитой от гонки между двумя Exists-проверками.
            throw new InvalidOperationException(
                "Запись с такими уникальными данными уже существует.",
                exception);
        }
    }

    /// <summary>
    /// Выполняет переданный бизнес-сценарий и его сохранение в одной транзакции базы данных.
    /// Метод нужен для атомарных операций вроде checkout: заказ, остатки и корзина либо изменяются вместе, либо откатываются вместе.
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            // Явный rollback показывает границу операции; исходная ошибка сохраняется.
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
