using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Абстрагирует бизнес-слой от конкретного канала отправки уведомлений.
/// </summary>
/// <remarks>
/// Интерфейс принадлежит BLL, потому что потребность «отправить уведомление»
/// возникает в бизнес-сценарии. Реализация остаётся в TelegramBot, поэтому BLL
/// не ссылается на Telegram.Bot и может работать в консольном режиме.
/// </remarks>
public interface INotificationSender
{
    /// <summary>Отправляет пользователю подготовленное уведомление.</summary>
    Task SendAsync(User user, Notification notification, CancellationToken cancellationToken);
}
