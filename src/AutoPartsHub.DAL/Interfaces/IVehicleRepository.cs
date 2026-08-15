using AutoPartsHub.Models;

namespace AutoPartsHub.DAL.Interfaces;

/// <summary>Определяет операции хранения автомобилей пользователя.</summary>
public interface IVehicleRepository
{
    /// <summary>Возвращает автомобили пользователя.</summary>
    Task<IReadOnlyCollection<Vehicle>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Находит автомобиль по нормализованному VIN.</summary>
    Task<Vehicle?> FindByVinAsync(string vin, CancellationToken cancellationToken);

    /// <summary>Добавляет автомобиль в контекст хранения.</summary>
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
}
