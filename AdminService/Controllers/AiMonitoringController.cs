using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/ai/alerts")]
public class AiMonitoringController(
    AdminDbContext dbContext,
    ICurrentAdminService currentAdminService) : AdminControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AiMonitoringAlertResponse>> Create(CreateAiMonitoringAlertRequest request)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.SourceService))
        {
            return BadRequest(new { message = "SourceService is required." });
        }

        if (string.IsNullOrWhiteSpace(request.AlertType))
        {
            return BadRequest(new { message = "AlertType is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        var alert = new AiMonitoringAlert
        {
            Id = Guid.NewGuid(),
            SourceService = request.SourceService.Trim(),
            AlertType = request.AlertType.Trim(),
            Severity = request.Severity,
            Message = request.Message.Trim(),
            DataJson = request.DataJson,
            Status = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AiMonitoringAlerts.Add(alert);
        AddActionLog(adminUserId.Value, "CreateAiMonitoringAlert", alert.Id, $"AI alert '{alert.AlertType}' was created from {alert.SourceService}.");

        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = alert.Id }, ToResponse(alert));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiMonitoringAlertResponse>>> GetAll(
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertSeverity? severity,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AiMonitoringAlerts.AsNoTracking();

        if (status is { } statusValue)
        {
            query = query.Where(alert => alert.Status == statusValue);
        }

        if (severity is { } severityValue)
        {
            query = query.Where(alert => alert.Severity == severityValue);
        }

        var alerts = await query
            .OrderByDescending(alert => alert.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return Ok(alerts.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AiMonitoringAlertResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var alert = await dbContext.AiMonitoringAlerts
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        return alert is null
            ? NotFound(new { message = "AI monitoring alert was not found." })
            : Ok(ToResponse(alert));
    }

    [HttpPost("{id:guid}/investigate")]
    public async Task<ActionResult<AiMonitoringAlertResponse>> Investigate(Guid id)
    {
        return await ChangeStatusAsync(id, AlertStatus.Investigating, "InvestigateAiMonitoringAlert", "AI monitoring alert investigation was started.");
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<AiMonitoringAlertResponse>> Resolve(Guid id)
    {
        return await ChangeStatusAsync(id, AlertStatus.Resolved, "ResolveAiMonitoringAlert", "AI monitoring alert was resolved.");
    }

    [HttpPost("{id:guid}/ignore")]
    public async Task<ActionResult<AiMonitoringAlertResponse>> Ignore(Guid id)
    {
        return await ChangeStatusAsync(id, AlertStatus.Ignored, "IgnoreAiMonitoringAlert", "AI monitoring alert was ignored.");
    }

    private async Task<ActionResult<AiMonitoringAlertResponse>> ChangeStatusAsync(
        Guid id,
        AlertStatus status,
        string action,
        string description)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var alert = await dbContext.AiMonitoringAlerts.FirstOrDefaultAsync(existing => existing.Id == id);
        if (alert is null)
        {
            return NotFound(new { message = "AI monitoring alert was not found." });
        }

        alert.Status = status;
        alert.ResolvedAt = status is AlertStatus.Resolved or AlertStatus.Ignored
            ? DateTime.UtcNow
            : null;

        AddActionLog(adminUserId.Value, action, alert.Id, description);

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(alert));
    }

    private void AddActionLog(Guid adminUserId, string action, Guid alertId, string description)
    {
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetService = "AdminService",
            TargetEntity = nameof(AiMonitoringAlert),
            TargetEntityId = alertId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static AiMonitoringAlertResponse ToResponse(AiMonitoringAlert alert)
    {
        return new AiMonitoringAlertResponse(
            alert.Id,
            alert.SourceService,
            alert.AlertType,
            alert.Severity,
            alert.Message,
            alert.DataJson,
            alert.Status,
            alert.CreatedAt,
            alert.ResolvedAt);
    }
}
