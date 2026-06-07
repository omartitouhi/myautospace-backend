using System.IdentityModel.Tokens.Jwt;
using ProviderService.Application.Interfaces;
using ProviderService.Application.Security;

namespace ProviderService.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public CurrentUser? GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var roles = user.FindAll("roles").Select(c => c.Value).ToList();

        return Guid.TryParse(userId, out var parsedUserId) && !string.IsNullOrWhiteSpace(email)
            ? new CurrentUser(parsedUserId, email, roles)
            : null;
    }
}
