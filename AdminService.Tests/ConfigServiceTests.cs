using AdminService.Application.DTOs;
using AdminService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Tests;

public class ConfigServiceTests
{
    [Fact]
    public async Task GetAllAsync_AndGetByKeyAsync_HideSensitiveConfigValue()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var service = new ConfigService(dbContext);
        var adminUserId = Guid.NewGuid();

        await service.UpsertAsync(
            "Jwt:Key",
            new UpdateSystemConfigRequest("secret-value", "JWT signing key", true),
            adminUserId);
        await service.UpsertAsync(
            "Feature:Enabled",
            new UpdateSystemConfigRequest("true", "Feature flag", false),
            adminUserId);

        var configs = await service.GetAllAsync();
        var sensitiveConfig = configs.Single(config => config.Key == "Jwt:Key");
        var publicConfig = configs.Single(config => config.Key == "Feature:Enabled");
        var byKey = await service.GetByKeyAsync("Jwt:Key");

        Assert.True(sensitiveConfig.IsSensitive);
        Assert.Null(sensitiveConfig.Value);
        Assert.Null(byKey?.Value);
        Assert.Equal("true", publicConfig.Value);
    }

    [Fact]
    public async Task UpsertAsync_CreatesAdminActionLog()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var service = new ConfigService(dbContext);
        var adminUserId = Guid.NewGuid();

        var response = await service.UpsertAsync(
            "Payments:Retries",
            new UpdateSystemConfigRequest("3", null, false),
            adminUserId);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal("CreateSystemConfig", actionLog.Action);
        Assert.Equal(adminUserId, actionLog.AdminUserId);
        Assert.Equal(response.Id, actionLog.TargetEntityId);
    }
}
