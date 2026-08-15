using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает оформление и просмотр заказов пользователя.
/// </summary>
public interface IOrderService
{
    /// <summary>Оформляет содержимое корзины.</summary>
    Task<OrderDto> CheckoutAsync(
        Guid userId,
        CheckoutRequest request,
        CancellationToken cancellationToken);

    /// <summary>Возвращает историю заказов пользователя.</summary>
    Task<IReadOnlyCollection<OrderDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Возвращает один заказ пользователя.</summary>
    Task<OrderDto> GetMineAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken);

    /// <summary>Находит заказ по человекочитаемому номеру.</summary>
    Task<OrderDto> FindByNumberAsync(
        Guid userId,
        string orderNumber,
        CancellationToken cancellationToken);
}
