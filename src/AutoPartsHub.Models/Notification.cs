using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит уведомление пользователя и результат попытки отправки.
/// </summary>
/// <remarks>
/// Создание и изменение статуса уведомления выполняет BLL, а модель хранит результат.
/// </remarks>
public class Notification : Entity
{
    /// <summary>Получает или задаёт внешний ключ пользователя.</summary>
    public Guid UserId { get; set; }

    /// <summary>Получает или задаёт тип уведомления.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Получает или задаёт текст уведомления.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Получает или задаёт статус отправки.</summary>
    public NotificationStatus Status { get; set; }

    /// <summary>Получает или задаёт дату создания.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Получает или задаёт дату успешной отправки.</summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>Получает или задаёт описание ошибки отправки.</summary>
    public string? Error { get; set; }

    /// <summary>Получает или задаёт пользователя по связи многие-к-одному.</summary>
    public User? User { get; set; }
}
