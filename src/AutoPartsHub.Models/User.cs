using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит данные пользователя AutoParts Hub, связанного с Telegram.
/// </summary>
/// <remarks>
/// Класс намеренно не содержит валидацию и изменение роли: по принятой
/// слоистой архитектуре модель описывает данные, а правила выполняет BLL.
/// </remarks>
public class User : Entity
{
    /// <summary>Получает или задаёт идентификатор чата Telegram.</summary>
    public long TelegramChatId { get; set; }

    /// <summary>Получает или задаёт отображаемое имя.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Получает или задаёт роль пользователя.</summary>
    public UserRole Role { get; set; }

    /// <summary>Получает или задаёт дату регистрации.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Получает или задаёт корзину по связи один-к-нулю-или-одному.</summary>
    public Cart? Cart { get; set; }

    /// <summary>Получает или задаёт автомобили по связи один-ко-многим.</summary>
    public ICollection<Vehicle> Vehicles { get; set; } = [];

    /// <summary>Получает или задаёт заказы по связи один-ко-многим.</summary>
    public ICollection<Order> Orders { get; set; } = [];

    /// <summary>Получает или задаёт товарные подписки по связи один-ко-многим.</summary>
    public ICollection<ProductSubscription> ProductSubscriptions { get; set; } = [];

    /// <summary>Получает или задаёт уведомления по связи один-ко-многим.</summary>
    public ICollection<Notification> Notifications { get; set; } = [];
}
