using AutoPartsHub.BLL.Contracts;
using AutoPartsHub.Core;

namespace AutoPartsHub.BLL;

public sealed class VehicleService(IAutoPartsRepository repository)
{
    public async Task<IReadOnlyCollection<VehicleDto>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var vehicles = await repository.GetVehiclesAsync(userId, cancellationToken);
        return vehicles.Select(ToDto).ToArray();
    }

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

    private static VehicleDto ToDto(Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Vin, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Engine);
}
