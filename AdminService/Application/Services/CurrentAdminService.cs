using System.IdentityModel.Tokens.Jwt;
using AdminService.Application.Interfaces;

namespace AdminService.Application.Services;

public class CurrentAdminService(IHttpContextAccessor httpContextAccessor) : ICurrentAdminService
{
    public Guid? GetAdminUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var adminUserId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(adminUserId, out var parsedAdminUserId)
            ? parsedAdminUserId
            : null;
    }
}
