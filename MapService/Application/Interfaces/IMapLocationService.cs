using MapService.Application.DTOs;
using MapService.Domain.Enums;

namespace MapService.Application.Interfaces;

public interface IMapLocationService
{
    Task<MapLocationResponse> CreateAsync(CreateLocationRequest request, Guid ownerAuthUserId);
    Task<MapLocationResponse> GetByIdAsync(Guid id);
    Task<MapLocationResponse> GetByEntityAsync(Guid entityId, EntityType entityType);
    Task<IReadOnlyCollection<MapLocationResponse>> GetByOwnerAsync(Guid ownerAuthUserId);
    Task<MapLocationResponse> UpdateAsync(Guid id, UpdateLocationRequest request, Guid ownerAuthUserId);
    Task DeleteAsync(Guid id, Guid ownerAuthUserId);
    Task<IReadOnlyCollection<NearbyLocationResult>> GetNearbyAsync(NearbySearchRequest request);
}
