using AutoPartsHub.BLL;
using AutoPartsHub.DTOs;
using AutoPartsHub.Models;
using AutoPartsHub.BLL.Interfaces;
using AutoPartsHub.BLL.Rules;
using AutoPartsHub.DAL.Interfaces;

namespace AutoPartsHub.BLL.Services;

/// <summary>
/// Управляет автомобилями пользователя для подбора совместимых товаров.
/// </summary>
/// <param name="vehicles">Хранилище автомобилей.</param>
/// <param name="unitOfWork">Граница сохранения изменений.</param>
public sealed class VehicleService(
    IVehicleRepository vehicles,
    IUnitOfWork unitOfWork) : IVehicleService
{
    /// <summary>
    /// Возвращает автомобили указанного пользователя.
    /// </summary>
    public async Task<IReadOnlyCollection<VehicleDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var items = await vehicles.GetByUserAsync(userId, cancellationToken);
        return items.Select(ToDto).ToArray();
    }

    /// <summary>
    /// Добавляет автомобиль после проверки и нормализации VIN.
    /// </summary>
    public async Task<VehicleDto> AddAsync(
        Guid userId,
        AddVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedVin = VehicleRules.NormalizeVin(request.Vin);
        if (await vehicles.FindByVinAsync(normalizedVin, cancellationToken) is not null)
            throw new ConflictException("Автомобиль с таким VIN уже добавлен.");

        var vehicle = VehicleRules.Create(
            userId,
            normalizedVin,
            request.Make,
            request.Model,
            request.Year,
            request.Engine);
        await vehicles.AddAsync(vehicle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(vehicle);
    }

    /// <summary>
    /// Преобразует доменный автомобиль в DTO.
    /// </summary>
    private static VehicleDto ToDto(Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Engine);
}
