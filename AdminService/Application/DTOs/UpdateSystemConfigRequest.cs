namespace AdminService.Application.DTOs;

public record UpdateSystemConfigRequest(string Value, string? Description, bool IsSensitive);
