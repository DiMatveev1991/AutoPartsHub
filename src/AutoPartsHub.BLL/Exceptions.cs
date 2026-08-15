namespace AutoPartsHub.BLL;

/// <summary>
/// Базовое исключение прикладного слоя с сообщением для пользователя.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Создаёт прикладное исключение с указанным сообщением.
    /// </summary>
    public AppException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт прикладное исключение с исходной причиной.
    /// </summary>
    public AppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Указывает, что входные данные или переход состояния нарушают бизнес-правило.
/// </summary>
/// <remarks>
/// Исключение находится в BLL, а не в Models: модели проекта намеренно являются
/// простыми объектами данных и не должны владеть поведением или зависеть от ошибок сценариев.
/// </remarks>
/// <param name="message">Понятное пользователю описание нарушенного правила.</param>
public sealed class DomainException(string message) : AppException(message);

/// <summary>
/// Указывает, что запрошенная сущность не найдена.
/// </summary>
/// <param name="message">Описание отсутствующей сущности.</param>
public sealed class NotFoundException(string message) : AppException(message);

/// <summary>
/// Указывает на конфликт с текущим состоянием данных.
/// </summary>
public sealed class ConflictException : AppException
{
    /// <summary>
    /// Создаёт исключение конфликта с указанным сообщением.
    /// </summary>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт исключение конфликта с исходной причиной.
    /// </summary>
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Указывает на ошибку аутентификации пользователя.
/// </summary>
/// <param name="message">Описание ошибки аутентификации.</param>
public sealed class AuthenticationException(string message) : AppException(message);

/// <summary>
/// Указывает на недостаточность прав для выполнения операции.
/// </summary>
/// <param name="message">Описание ограничения доступа.</param>
public sealed class ForbiddenException(string message) : AppException(message);
