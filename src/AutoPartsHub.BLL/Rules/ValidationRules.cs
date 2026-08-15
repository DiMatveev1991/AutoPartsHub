namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Содержит повторно используемые проверки простых входных значений.
/// </summary>
/// <remarks>
/// Класс не знает ни о базе, ни о Telegram, ни о конкретной сущности. Он вынесен
/// отдельно, чтобы правила товара, заказа и пользователя не дублировали нормализацию строк.
/// </remarks>
internal static class ValidationRules
{
    /// <summary>Проверяет обязательную строку, обрезает пробелы и ограничивает длину.</summary>
    internal static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException(
                $"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }

    /// <summary>Нормализует необязательную строку без превращения пустого значения в данные.</summary>
    internal static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
