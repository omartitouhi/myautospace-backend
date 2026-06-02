using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Application.Services;

public class ConfigService(AdminDbContext dbContext) : IConfigService
{
    public async Task<IReadOnlyList<SystemConfigResponse>> GetAllAsync()
    {
        var configs = await dbContext.SystemConfigs
            .AsNoTracking()
            .OrderBy(config => config.Key)
            .ToListAsync();

        return configs.Select(ToResponse).ToList();
    }

    public async Task<SystemConfigResponse?> GetByKeyAsync(string key)
    {
        key = key.Trim();

        var config = await dbContext.SystemConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(systemConfig => systemConfig.Key == key);

        return config is null ? null : ToResponse(config);
    }

    public async Task<SystemConfigResponse> UpsertAsync(string key, UpdateSystemConfigRequest request, Guid adminUserId)
    {
        key = key.Trim();

        var config = await dbContext.SystemConfigs.FirstOrDefaultAsync(systemConfig => systemConfig.Key == key);
        var action = "UpdateSystemConfig";

        if (config is null)
        {
            action = "CreateSystemConfig";
            config = new SystemConfig
            {
                Id = Guid.NewGuid(),
                Key = key
            };
            dbContext.SystemConfigs.Add(config);
        }

        config.Value = request.Value.Trim();
        config.Description = request.Description?.Trim();
        config.IsSensitive = request.IsSensitive;
        config.UpdatedAt = DateTime.UtcNow;

        AddActionLog(adminUserId, action, config.Id, $"Configuration '{key}' was saved.");

        await dbContext.SaveChangesAsync();
        return ToResponse(config);
    }

    public async Task<bool> DeleteAsync(string key, Guid adminUserId)
    {
        key = key.Trim();

        var config = await dbContext.SystemConfigs.FirstOrDefaultAsync(systemConfig => systemConfig.Key == key);
        if (config is null)
        {
            return false;
        }

        dbContext.SystemConfigs.Remove(config);
        AddActionLog(adminUserId, "DeleteSystemConfig", config.Id, $"Configuration '{key}' was deleted.");

        await dbContext.SaveChangesAsync();
        return true;
    }

    private void AddActionLog(Guid adminUserId, string action, Guid configId, string description)
    {
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetService = "AdminService",
            TargetEntity = nameof(SystemConfig),
            TargetEntityId = configId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static SystemConfigResponse ToResponse(SystemConfig config)
    {
        return new SystemConfigResponse(
            config.Id,
            config.Key,
            config.IsSensitive ? null : config.Value,
            config.Description,
            config.IsSensitive,
            config.UpdatedAt);
    }
}
