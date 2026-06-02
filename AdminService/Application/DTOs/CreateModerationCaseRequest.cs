namespace AdminService.Application.DTOs;

public record CreateModerationCaseRequest(
    string ReportedEntityType,
    Guid ReportedEntityId,
    Guid? ReportedByUserId,
    string Reason);
