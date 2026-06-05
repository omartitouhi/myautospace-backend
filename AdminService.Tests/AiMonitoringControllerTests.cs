using AdminService.Application.DTOs;
using AdminService.Controllers;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Tests;

public class AiMonitoringControllerTests
{
    [Theory]
    [InlineData("investigate", AlertStatus.Investigating, "InvestigateAiMonitoringAlert", false)]
    [InlineData("resolve", AlertStatus.Resolved, "ResolveAiMonitoringAlert", true)]
    [InlineData("ignore", AlertStatus.Ignored, "IgnoreAiMonitoringAlert", true)]
    public async Task StatusActions_ChangeAlertStatusAndCreateAdminActionLog(
        string operation,
        AlertStatus expectedStatus,
        string expectedAction,
        bool expectsResolvedAt)
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var adminUserId = Guid.NewGuid();
        var alert = new AiMonitoringAlert
        {
            Id = Guid.NewGuid(),
            SourceService = "FraudService",
            AlertType = "SuspiciousBooking",
            Severity = AlertSeverity.High,
            Message = "Suspicious booking detected",
            Status = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.AiMonitoringAlerts.Add(alert);
        await dbContext.SaveChangesAsync();

        var controller = new AiMonitoringController(dbContext, new FakeCurrentAdminService(adminUserId));
        var result = operation switch
        {
            "investigate" => await controller.Investigate(alert.Id),
            "resolve" => await controller.Resolve(alert.Id),
            _ => await controller.Ignore(alert.Id)
        };

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AiMonitoringAlertResponse>(okResult.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal(expectsResolvedAt, response.ResolvedAt is not null);

        var savedAlert = await dbContext.AiMonitoringAlerts.SingleAsync();
        Assert.Equal(expectedStatus, savedAlert.Status);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal(adminUserId, actionLog.AdminUserId);
        Assert.Equal(expectedAction, actionLog.Action);
        Assert.Equal(alert.Id, actionLog.TargetEntityId);
    }

    [Fact]
    public async Task Create_AddsOpenAlertAndAdminActionLog()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var adminUserId = Guid.NewGuid();
        var controller = new AiMonitoringController(dbContext, new FakeCurrentAdminService(adminUserId));

        var result = await controller.Create(new CreateAiMonitoringAlertRequest(
            "FraudService",
            "RiskScoreExceeded",
            AlertSeverity.Critical,
            "Risk score exceeded threshold",
            "{\"score\":99}"));

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<AiMonitoringAlertResponse>(createdResult.Value);
        Assert.Equal(AlertStatus.Open, response.Status);

        var actionLog = await dbContext.AdminActionLogs.SingleAsync();
        Assert.Equal("CreateAiMonitoringAlert", actionLog.Action);
        Assert.Equal(response.Id, actionLog.TargetEntityId);
    }
}
