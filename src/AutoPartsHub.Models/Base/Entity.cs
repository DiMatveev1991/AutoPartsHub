namespace AutoPartsHub.Models.Base;

/// <summary>
/// Задаёт общий идентификатор сущностей с одиночным ключом Guid.
/// </summary>
/// <remarks>
/// Базовый класс повторяет подход DeliveryApp и устраняет дублирование Id.
/// Он содержит только данные; CartItem не наследуется от него, потому что имеет составной ключ.
/// </remarks>
public abstract class Entity
{
    /// <summary>Получает или задаёт уникальный идентификатор сущности.</summary>
    public Guid Id { get; set; }
}
