namespace AdminService.Application.DTOs;

public record SystemConfigResponse(
    Guid Id,
    string Key,
    string? Value,
    string? Description,
    bool IsSensitive,
    DateTime UpdatedAt);
