using AdminService.Application.DTOs;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/audit")]
public class AuditController(AdminDbContext dbContext) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminActionLogResponse>>> GetAll(
        [FromQuery] Guid? adminUserId,
        [FromQuery] string? targetService,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate is { } from && toDate is { } to && from > to)
        {
            return BadRequest(new { message = "fromDate must be earlier than or equal to toDate." });
        }

        var query = dbContext.AdminActionLogs.AsNoTracking();

        if (adminUserId is { } adminId)
        {
            query = query.Where(log => log.AdminUserId == adminId);
        }

        if (!string.IsNullOrWhiteSpace(targetService))
        {
            var service = targetService.Trim();
            query = query.Where(log => log.TargetService == service);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionName = action.Trim();
            query = query.Where(log => log.Action == actionName);
        }

        if (fromDate is { } fromDateValue)
        {
            query = query.Where(log => log.CreatedAt >= ToUtc(fromDateValue));
        }

        if (toDate is { } toDateValue)
        {
            query = query.Where(log => log.CreatedAt <= ToUtc(toDateValue));
        }

        var logs = await query
            .OrderByDescending(log => log.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return Ok(logs.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminActionLogResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var log = await dbContext.AdminActionLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        return log is null
            ? NotFound(new { message = "Audit log was not found." })
            : Ok(ToResponse(log));
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static AdminActionLogResponse ToResponse(AdminActionLog log)
    {
        return new AdminActionLogResponse(
            log.Id,
            log.AdminUserId,
            log.Action,
            log.TargetService,
            log.TargetEntity,
            log.TargetEntityId,
            log.Description,
            log.CreatedAt);
    }
}
