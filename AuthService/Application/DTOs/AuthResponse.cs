namespace AuthService.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles);
