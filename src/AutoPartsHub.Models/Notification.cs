namespace AutoPartsHub.Models;

/// <summary>
/// Представляет уведомление пользователя и результат его отправки.
/// </summary>
/// <remarks>
/// Связь многие-к-одному: много уведомлений может принадлежать одному пользователю.
/// </remarks>
public sealed class Notification
{
    /// <summary>
    /// Создаёт экземпляр уведомления для восстановления Entity Framework Core.
    /// </summary>
    private Notification()
    {
    }

    /// <summary>
    /// Создаёт ожидающее отправки уведомление.
    /// </summary>
    public Notification(Guid userId, string type, string text, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = Required(type, nameof(type), 60);
        Text = Required(text, nameof(text), 1000);
        Status = NotificationStatus.Pending;
        CreatedAt = createdAt;
    }

    /// <summary>Получает уникальный идентификатор уведомления.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает внешний ключ пользователя; один пользователь получает много уведомлений.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает тип уведомления.</summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>Получает текст уведомления.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>Получает текущий статус отправки.</summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>Получает дату и время создания уведомления.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Получает дату и время успешной отправки.</summary>
    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>Получает описание ошибки последней попытки отправки.</summary>
    public string? Error { get; private set; }

    /// <summary>Получает сторону «один» связи многие-к-одному с пользователем.</summary>
    public User? User { get; private set; }

    /// <summary>
    /// Отмечает уведомление успешно отправленным.
    /// </summary>
    public void MarkSent(DateTimeOffset sentAt)
    {
        Status = NotificationStatus.Sent;
        SentAt = sentAt;
        Error = null;
    }

    /// <summary>
    /// Сохраняет безопасно ограниченное описание ошибки отправки.
    /// </summary>
    public void MarkFailed(string error)
    {
        // Failed не выбирается повторно фоновым worker: так постоянная ошибка
        // Telegram не создаёт бесконечный цикл. Причина остаётся для диагностики.
        Status = NotificationStatus.Failed;
        Error = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error[..Math.Min(error.Length, 1000)];
    }

    /// <summary>
    /// Проверяет обязательную строку, удаляет крайние пробелы и контролирует длину.
    /// </summary>
    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}
