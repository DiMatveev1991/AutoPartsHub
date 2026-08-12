using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

/// <summary>
/// Управляет автомобилями пользователя для подбора совместимых товаров.
/// </summary>
/// <param name="repository">Хранилище данных приложения.</param>
public sealed class VehicleService(IAutoPartsRepository repository)
{
    /// <summary>
    /// Возвращает автомобили указанного пользователя.
    /// </summary>
    public async Task<IReadOnlyCollection<VehicleDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var vehicles = await repository.GetVehiclesAsync(userId, cancellationToken);
        return vehicles.Select(ToDto).ToArray();
    }

    /// <summary>
    /// Добавляет автомобиль после проверки и нормализации VIN.
    /// </summary>
    public async Task<VehicleDto> AddAsync(
        Guid userId,
        AddVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedVin = Vehicle.NormalizeVin(request.Vin);
        if (await repository.FindVehicleByVinAsync(normalizedVin, cancellationToken) is not null)
            throw new ConflictException("Автомобиль с таким VIN уже добавлен.");

        var vehicle = new Vehicle(
            userId,
            normalizedVin,
            request.Make,
            request.Model,
            request.Year,
            request.Engine);
        await repository.AddVehicleAsync(vehicle, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(vehicle);
    }

    /// <summary>
    /// Преобразует доменный автомобиль в DTO.
    /// </summary>
    private static VehicleDto ToDto(Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Engine);
}
