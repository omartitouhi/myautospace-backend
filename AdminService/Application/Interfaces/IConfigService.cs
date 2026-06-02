using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IConfigService
{
    Task<IReadOnlyList<SystemConfigResponse>> GetAllAsync();

    Task<SystemConfigResponse?> GetByKeyAsync(string key);

    Task<SystemConfigResponse> UpsertAsync(string key, UpdateSystemConfigRequest request, Guid adminUserId);

    Task<bool> DeleteAsync(string key, Guid adminUserId);
}
