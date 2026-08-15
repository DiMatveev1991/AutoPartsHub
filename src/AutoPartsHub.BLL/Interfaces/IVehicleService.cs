using AutoPartsHub.DTOs;

namespace AutoPartsHub.BLL.Interfaces;

/// <summary>
/// Описывает операции с автомобилями пользователя.
/// </summary>
public interface IVehicleService
{
    /// <summary>Возвращает сохранённые автомобили пользователя.</summary>
    Task<IReadOnlyCollection<VehicleDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Добавляет автомобиль пользователя.</summary>
    Task<VehicleDto> AddAsync(
        Guid userId,
        AddVehicleRequest request,
        CancellationToken cancellationToken);
}
