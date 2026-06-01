using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/trust-score")]
public class TrustScoreController(ITrustScoreService trustScoreService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TrustScoreResponse>> GetTrustScore()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var trustScore = await trustScoreService.GetAsync(authUserId.Value);
        return trustScore is null
            ? NotFound(new { message = "Trust score was not found." })
            : Ok(ToResponse(trustScore));
    }

    [HttpPost("recalculate")]
    public async Task<ActionResult<TrustScoreResponse>> Recalculate()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var trustScore = await trustScoreService.RecalculateAsync(authUserId.Value);
        return trustScore is null
            ? NotFound(new { message = "User profile was not found." })
            : Ok(ToResponse(trustScore));
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private static TrustScoreResponse ToResponse(Domain.Entities.TrustScore trustScore)
    {
        return new TrustScoreResponse(
            trustScore.Id,
            trustScore.Score,
            trustScore.LastCalculatedAt);
    }
}
