using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, List<string> roles);

    string GenerateRefreshToken();
}
