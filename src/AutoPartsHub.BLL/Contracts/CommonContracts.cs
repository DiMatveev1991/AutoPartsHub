namespace AutoPartsHub.BLL.Contracts;

/// <summary>
/// Представляет одну страницу результатов и сведения о пагинации.
/// </summary>
/// <typeparam name="T">Тип элемента результата.</typeparam>
/// <param name="Items">Элементы текущей страницы.</param>
/// <param name="Page">Номер текущей страницы.</param>
/// <param name="PageSize">Размер страницы.</param>
/// <param name="TotalCount">Общее количество элементов.</param>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>Получает общее количество страниц.</summary>
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
