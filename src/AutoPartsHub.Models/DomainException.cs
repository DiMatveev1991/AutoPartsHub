namespace AutoPartsHub.Models;

/// <summary>
/// Представляет нарушение бизнес-правил доменной модели.
/// </summary>
/// <param name="message">Понятное пользователю описание ошибки.</param>
public sealed class DomainException(string message) : Exception(message);
