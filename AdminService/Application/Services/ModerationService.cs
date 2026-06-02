using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Application.Services;

public class ModerationService(AdminDbContext dbContext) : IModerationService
{
    public async Task<ModerationCase> CreateAsync(CreateModerationCaseRequest request, Guid adminUserId)
    {
        var moderationCase = new ModerationCase
        {
            Id = Guid.NewGuid(),
            ReportedEntityType = request.ReportedEntityType,
            ReportedEntityId = request.ReportedEntityId,
            ReportedByUserId = request.ReportedByUserId,
            Status = ModerationStatus.Pending,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ModerationCases.Add(moderationCase);
        AddActionLog(adminUserId, "CreateModerationCase", moderationCase.Id, $"Created moderation case for {request.ReportedEntityType}.");

        await dbContext.SaveChangesAsync();
        return moderationCase;
    }

    public async Task<IReadOnlyList<ModerationCase>> GetAllAsync()
    {
        return await dbContext.ModerationCases
            .AsNoTracking()
            .OrderByDescending(moderationCase => moderationCase.CreatedAt)
            .ToListAsync();
    }

    public async Task<ModerationCase?> GetByIdAsync(Guid id)
    {
        return await dbContext.ModerationCases
            .AsNoTracking()
            .FirstOrDefaultAsync(moderationCase => moderationCase.Id == id);
    }

    public async Task<ModerationCase?> AssignAsync(Guid id, AssignModerationCaseRequest request, Guid adminUserId)
    {
        var moderationCase = await dbContext.ModerationCases.FindAsync(id);
        if (moderationCase is null)
        {
            return null;
        }

        moderationCase.AssignedAdminId = request.AssignedAdminId;
        moderationCase.Status = ModerationStatus.InReview;
        AddActionLog(adminUserId, "AssignModerationCase", moderationCase.Id, $"Assigned moderation case to admin {request.AssignedAdminId}.");

        await dbContext.SaveChangesAsync();
        return moderationCase;
    }

    public Task<ModerationCase?> ApproveAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId)
    {
        return CompleteAsync(id, ModerationStatus.Approved, request.Decision, adminUserId, "ApproveModerationCase");
    }

    public Task<ModerationCase?> RejectAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId)
    {
        return CompleteAsync(id, ModerationStatus.Rejected, request.Decision, adminUserId, "RejectModerationCase");
    }

    public Task<ModerationCase?> ResolveAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId)
    {
        return CompleteAsync(id, ModerationStatus.Resolved, request.Decision, adminUserId, "ResolveModerationCase");
    }

    private async Task<ModerationCase?> CompleteAsync(
        Guid id,
        ModerationStatus status,
        string? decision,
        Guid adminUserId,
        string action)
    {
        var moderationCase = await dbContext.ModerationCases.FindAsync(id);
        if (moderationCase is null)
        {
            return null;
        }

        moderationCase.Status = status;
        moderationCase.Decision = decision;
        moderationCase.ResolvedAt = DateTime.UtcNow;
        AddActionLog(adminUserId, action, moderationCase.Id, $"Set moderation case status to {status}.");

        await dbContext.SaveChangesAsync();
        return moderationCase;
    }

    private void AddActionLog(Guid adminUserId, string action, Guid moderationCaseId, string description)
    {
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetService = "AdminService",
            TargetEntity = nameof(ModerationCase),
            TargetEntityId = moderationCaseId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }
}
