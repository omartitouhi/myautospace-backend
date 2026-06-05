using AdminService.Application.DTOs;
using AdminService.Application.Services;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Tests;

public class ModerationServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPendingModerationCaseAndActionLog()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var service = new ModerationService(dbContext);
        var adminUserId = Guid.NewGuid();
        var reportedEntityId = Guid.NewGuid();
        var reportedByUserId = Guid.NewGuid();

        var moderationCase = await service.CreateAsync(
            new CreateModerationCaseRequest("Vehicle", reportedEntityId, reportedByUserId, "Suspicious listing"),
            adminUserId);

        Assert.NotEqual(Guid.Empty, moderationCase.Id);
        Assert.Equal("Vehicle", moderationCase.ReportedEntityType);
        Assert.Equal(reportedEntityId, moderationCase.ReportedEntityId);
        Assert.Equal(reportedByUserId, moderationCase.ReportedByUserId);
        Assert.Equal(ModerationStatus.Pending, moderationCase.Status);
        Assert.Null(moderationCase.AssignedAdminId);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal(adminUserId, actionLog.AdminUserId);
        Assert.Equal("CreateModerationCase", actionLog.Action);
        Assert.Equal(nameof(ModerationCase), actionLog.TargetEntity);
        Assert.Equal(moderationCase.Id, actionLog.TargetEntityId);
    }

    [Fact]
    public async Task AssignAsync_AssignsCaseToAdminAndCreatesActionLog()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var service = new ModerationService(dbContext);
        var actingAdminId = Guid.NewGuid();
        var assignedAdminId = Guid.NewGuid();
        var moderationCase = await service.CreateAsync(
            new CreateModerationCaseRequest("User", Guid.NewGuid(), null, "Abuse report"),
            actingAdminId);
        dbContext.AdminActionLogs.RemoveRange(dbContext.AdminActionLogs);
        await dbContext.SaveChangesAsync();

        var result = await service.AssignAsync(
            moderationCase.Id,
            new AssignModerationCaseRequest(assignedAdminId),
            actingAdminId);

        Assert.NotNull(result);
        Assert.Equal(assignedAdminId, result.AssignedAdminId);
        Assert.Equal(ModerationStatus.InReview, result.Status);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal("AssignModerationCase", actionLog.Action);
        Assert.Equal(moderationCase.Id, actionLog.TargetEntityId);
    }

    [Theory]
    [InlineData("approve", ModerationStatus.Approved, "ApproveModerationCase")]
    [InlineData("reject", ModerationStatus.Rejected, "RejectModerationCase")]
    [InlineData("resolve", ModerationStatus.Resolved, "ResolveModerationCase")]
    public async Task CompleteActions_ChangeStatusAndCreateActionLog(
        string operation,
        ModerationStatus expectedStatus,
        string expectedAction)
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var service = new ModerationService(dbContext);
        var adminUserId = Guid.NewGuid();
        var moderationCase = await service.CreateAsync(
            new CreateModerationCaseRequest("Review", Guid.NewGuid(), null, "Needs decision"),
            adminUserId);
        dbContext.AdminActionLogs.RemoveRange(dbContext.AdminActionLogs);
        await dbContext.SaveChangesAsync();

        var decision = new ModerationDecisionRequest("Decision note");
        var result = operation switch
        {
            "approve" => await service.ApproveAsync(moderationCase.Id, decision, adminUserId),
            "reject" => await service.RejectAsync(moderationCase.Id, decision, adminUserId),
            _ => await service.ResolveAsync(moderationCase.Id, decision, adminUserId)
        };

        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal("Decision note", result.Decision);
        Assert.NotNull(result.ResolvedAt);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal(expectedAction, actionLog.Action);
        Assert.Equal(moderationCase.Id, actionLog.TargetEntityId);
    }
}
