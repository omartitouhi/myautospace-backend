namespace AdminService.Application.DTOs;

public record AdminActionLogResponse(
    Guid Id,
    Guid AdminUserId,
    string Action,
    string TargetService,
    string TargetEntity,
    Guid? TargetEntityId,
    string Description,
    DateTime CreatedAt);
