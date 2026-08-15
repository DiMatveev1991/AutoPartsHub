using AutoPartsHub.Models.Base;

namespace AutoPartsHub.Models;

/// <summary>
/// Хранит карточку товара, складской остаток и состояние публикации.
/// </summary>
/// <remarks>
/// Расчёты, резервирование, деактивация и валидация выполняются BLL.
/// Коллекция совместимости является только навигационными данными EF Core.
/// </remarks>
public class Product : Entity
{
    /// <summary>Получает или задаёт внешний ключ категории.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Получает или задаёт нормализованный артикул.</summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>Получает или задаёт название товара.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Получает или задаёт подробное описание.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Получает или задаёт состояние товара.</summary>
    public ProductCondition Condition { get; set; }

    /// <summary>Получает или задаёт цену товара.</summary>
    public decimal Price { get; set; }

    /// <summary>Получает или задаёт доступный остаток.</summary>
    public int Stock { get; set; }

    /// <summary>Получает или задаёт признак доступности в каталоге.</summary>
    public bool IsActive { get; set; }

    /// <summary>Получает или задаёт токен оптимистичной блокировки.</summary>
    public Guid ConcurrencyToken { get; set; }

    /// <summary>Получает или задаёт дату создания товара.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Получает или задаёт дату последнего изменения.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Получает или задаёт категорию по связи многие-к-одному.</summary>
    public Category? Category { get; set; }

    /// <summary>Получает или задаёт правила совместимости товара.</summary>
    public ICollection<ProductCompatibility> Compatibilities { get; set; } = [];

    /// <summary>Получает или задаёт позиции корзин по связи один-ко-многим.</summary>
    public ICollection<CartItem> CartItems { get; set; } = [];

    /// <summary>Получает или задаёт позиции заказов по связи один-ко-многим.</summary>
    public ICollection<OrderItem> OrderItems { get; set; } = [];

    /// <summary>Получает или задаёт подписки по связи один-ко-многим.</summary>
    public ICollection<ProductSubscription> ProductSubscriptions { get; set; } = [];
}
