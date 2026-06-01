using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/activity")]
public class ActivityController(IUserActivityService userActivityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserActivityResponse>>> GetActivities()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var activities = await userActivityService.GetActivitiesAsync(authUserId.Value);

        return Ok(activities
            .Select(activity => new UserActivityResponse(
                activity.Id,
                activity.Action,
                activity.Description,
                activity.CreatedAt))
            .ToList());
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }
}
