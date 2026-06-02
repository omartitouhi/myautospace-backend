using VehicleService.Application.DTOs;

namespace VehicleService.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, Guid ownerAuthUserId);
    Task<VehicleResponse> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<VehicleSummaryResponse>> GetAllActiveAsync();
    Task<IReadOnlyCollection<VehicleSummaryResponse>> GetByOwnerAsync(Guid ownerAuthUserId);
    Task<VehicleResponse> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid ownerAuthUserId);
    Task UpdateStatusAsync(Guid id, UpdateVehicleStatusRequest request, Guid ownerAuthUserId);
    Task DeleteAsync(Guid id, Guid ownerAuthUserId);
}
