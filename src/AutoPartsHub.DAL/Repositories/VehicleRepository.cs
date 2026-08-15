using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение автомобилей через EF Core.</summary>
internal sealed class VehicleRepository(AutoPartsDbContext db) : IVehicleRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Vehicle>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Vehicles
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Make)
            .ThenBy(item => item.Model)
            .ToArrayAsync(cancellationToken);

    /// <inheritdoc />
    public Task<Vehicle?> FindByVinAsync(string vin, CancellationToken cancellationToken) =>
        db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Vin == vin, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
        await db.Vehicles.AddAsync(vehicle, cancellationToken);
}
