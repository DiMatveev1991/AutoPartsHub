namespace AutoPartsHub.DAL.Interfaces;

/// <summary>
/// Задаёт общую границу сохранения и транзакции для специализированных репозиториев.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Сохраняет все изменения текущего scope.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Выполняет согласованное изменение нескольких репозиториев в транзакции.</summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken);
}
