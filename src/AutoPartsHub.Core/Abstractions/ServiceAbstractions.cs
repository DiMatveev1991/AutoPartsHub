namespace AutoPartsHub.Core;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IOrderNumberGenerator
{
    string Next(DateTimeOffset now);
}

public interface INotificationSender
{
    Task SendAsync(User user, Notification notification, CancellationToken cancellationToken);
}
