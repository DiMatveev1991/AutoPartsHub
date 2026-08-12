namespace AutoPartsHub.Core;

public sealed class Notification
{
    private Notification()
    {
    }

    public Notification(Guid userId, string type, string text, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = Required(type, nameof(type), 60);
        Text = Required(text, nameof(text), 1000);
        Status = NotificationStatus.Pending;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public string? Error { get; private set; }
    public User? User { get; private set; }

    public void MarkSent(DateTimeOffset sentAt)
    {
        Status = NotificationStatus.Sent;
        SentAt = sentAt;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = NotificationStatus.Failed;
        Error = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error[..Math.Min(error.Length, 1000)];
    }

    private static string Required(string value, string name, int maxLength)
    {
        var result = value?.Trim();
        if (string.IsNullOrWhiteSpace(result) || result.Length > maxLength)
            throw new DomainException($"Поле {name} обязательно и не должно превышать {maxLength} символов.");
        return result;
    }
}
