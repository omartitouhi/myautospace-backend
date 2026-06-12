using VehicleService.Domain.Entities;

namespace VehicleService.Application.Interfaces;

/// <summary>
/// Keeps SearchService's vehicle index in sync with the catalogue.
/// Implementations are best-effort: failures are logged, never thrown, so a
/// search outage cannot break vehicle operations.
/// </summary>
public interface ISearchIndexClient
{
    Task UpsertAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
