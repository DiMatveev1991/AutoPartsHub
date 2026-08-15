using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит подписку пользователя на изменение состояния товара.
/// </summary>
/// <remarks>
/// Проверка параметров, определение срабатывания и завершение подписки выполняются BLL.
/// </remarks>
public class ProductSubscription : Entity
{
    /// <summary>Получает или задаёт внешний ключ пользователя.</summary>
    public Guid UserId { get; set; }

    /// <summary>Получает или задаёт внешний ключ товара.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Получает или задаёт тип подписки.</summary>
    public SubscriptionType Type { get; set; }

    /// <summary>Получает или задаёт целевую цену.</summary>
    public decimal? TargetPrice { get; set; }

    /// <summary>Получает или задаёт признак ожидания срабатывания.</summary>
    public bool IsActive { get; set; }

    /// <summary>Получает или задаёт дату создания.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Получает или задаёт пользователя.</summary>
    public User? User { get; set; }

    /// <summary>Получает или задаёт отслеживаемый товар.</summary>
    public Product? Product { get; set; }
}
