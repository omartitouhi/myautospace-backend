using AdminService.Application.DTOs;
using AdminService.Controllers;
using AdminService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Tests;

public class AuditControllerTests
{
    [Fact]
    public async Task GetAll_FiltersByAdminServiceActionAndDateRange()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var matchingAdminId = Guid.NewGuid();
        var otherAdminId = Guid.NewGuid();
        var fromDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var toDate = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        dbContext.AdminActionLogs.AddRange(
            NewLog(matchingAdminId, "AdminService", "ResolveModerationCase", new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
            NewLog(matchingAdminId, "AdminService", "CreateModerationCase", new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
            NewLog(otherAdminId, "AdminService", "ResolveModerationCase", new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
            NewLog(matchingAdminId, "PaymentService", "ResolveModerationCase", new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),
            NewLog(matchingAdminId, "AdminService", "ResolveModerationCase", new DateTime(2026, 1, 25, 0, 0, 0, DateTimeKind.Utc)));
        await dbContext.SaveChangesAsync();

        var controller = new AuditController(dbContext);
        var result = await controller.GetAll(
            matchingAdminId,
            "AdminService",
            "ResolveModerationCase",
            fromDate,
            toDate,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var logs = Assert.IsAssignableFrom<IReadOnlyList<AdminActionLogResponse>>(okResult.Value);
        var log = Assert.Single(logs);
        Assert.Equal(matchingAdminId, log.AdminUserId);
        Assert.Equal("AdminService", log.TargetService);
        Assert.Equal("ResolveModerationCase", log.Action);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequestWhenFromDateIsAfterToDate()
    {
        await using var dbContext = TestAdminDbContextFactory.Create();
        var controller = new AuditController(dbContext);

        var result = await controller.GetAll(
            null,
            null,
            null,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    private static AdminActionLog NewLog(Guid adminUserId, string targetService, string action, DateTime createdAt)
    {
        return new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetService = targetService,
            TargetEntity = "Entity",
            Description = "Description",
            CreatedAt = createdAt
        };
    }
}
