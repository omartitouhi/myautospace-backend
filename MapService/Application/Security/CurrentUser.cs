namespace MapService.Application.Security;

public record CurrentUser(
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles);
