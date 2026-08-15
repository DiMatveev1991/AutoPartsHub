using AutoPartsHub.DAL.Context;
using AutoPartsHub.DAL.Interfaces;
using AutoPartsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsHub.DAL.Repositories;

/// <summary>Реализует хранение автомобилей через EF Core.</summary>
internal sealed class VehicleRepository(AutoPartsDbContext db) : IVehicleRepository
{
    /// <summary>
    /// Возвращает автомобили пользователя в стабильном порядке без отслеживания EF Core.
    /// Список используется только для показа, поэтому отключение Change Tracker уменьшает накладные расходы.
    /// </summary>
    public async Task<IReadOnlyCollection<Vehicle>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.Vehicles
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Make)
            .ThenBy(item => item.Model)
            .ToArrayAsync(cancellationToken);

    /// <summary>
    /// Находит автомобиль по нормализованному VIN без отслеживания, поскольку запрос нужен для проверки уникальности.
    /// Нормализация остаётся ответственностью BLL, а репозиторий выполняет точное сравнение в базе данных.
    /// </summary>
    public Task<Vehicle?> FindByVinAsync(string vin, CancellationToken cancellationToken) =>
        db.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Vin == vin, cancellationToken);

    /// <summary>
    /// Добавляет автомобиль в Change Tracker, не сохраняя его отдельно от бизнес-сценария.
    /// Окончательный commit выполняет Unit of Work после завершения всех проверок BLL.
    /// </summary>
    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
        await db.Vehicles.AddAsync(vehicle, cancellationToken);
}
