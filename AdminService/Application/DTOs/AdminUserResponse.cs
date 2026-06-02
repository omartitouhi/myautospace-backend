namespace AdminService.Application.DTOs;

public record AdminUserResponse(
    Guid Id,
    string Email,
    string Status,
    IReadOnlyList<string> Roles);
