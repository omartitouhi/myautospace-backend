using AdminService.Domain.Enums;

namespace AdminService.Application.DTOs;

public record ModerationCaseResponse(
    Guid Id,
    string ReportedEntityType,
    Guid ReportedEntityId,
    Guid? ReportedByUserId,
    Guid? AssignedAdminId,
    ModerationStatus Status,
    string Reason,
    string? Decision,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
