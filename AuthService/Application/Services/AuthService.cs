using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class AuthService(
    AuthDbContext dbContext,
    IJwtService jwtService,
    IConfiguration configuration) : IAuthService
{
    private const string DefaultRoleName = "Buyer";

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var emailExists = await dbContext.Users.AnyAsync(user => user.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var role = await dbContext.Roles.FirstOrDefaultAsync(role => role.Name == DefaultRoleName);

        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = DefaultRoleName
            };

            dbContext.Roles.Add(role);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = role.Id,
            Role = role
        });

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return await CreateAuthResponseAsync(user, new List<string> { DefaultRoleName });
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToList();

        return await CreateAuthResponseAsync(user, roles);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .ThenInclude(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == request.RefreshToken);

        if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Refresh token is invalid or expired.");
        }

        refreshToken.IsRevoked = true;

        var roles = refreshToken.User.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToList();

        return await CreateAuthResponseAsync(refreshToken.User, roles);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == request.RefreshToken);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.IsRevoked = true;
        await dbContext.SaveChangesAsync();
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, List<string> roles)
    {
        var accessToken = jwtService.GenerateAccessToken(user, roles);
        var refreshTokenValue = jwtService.GenerateRefreshToken();
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes());

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            accessTokenExpiresAt,
            user.Id,
            user.Email,
            roles);
    }

    private int GetAccessTokenExpirationMinutes()
    {
        return configuration.GetSection("Jwt").GetValue<int>("AccessTokenExpirationMinutes");
    }

    private int GetRefreshTokenExpirationDays()
    {
        return configuration.GetSection("Jwt").GetValue<int>("RefreshTokenExpirationDays");
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
