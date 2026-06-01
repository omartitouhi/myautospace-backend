using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/preferences")]
public class PreferencesController(UserDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserPreferenceResponse>> GetPreferences()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.UserPreference)
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        return userProfile.UserPreference is null
            ? NotFound(new { message = "User preferences were not found." })
            : Ok(ToResponse(userProfile.UserPreference));
    }

    [HttpPut]
    public async Task<ActionResult<UserPreferenceResponse>> UpdatePreferences(UpdatePreferencesRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.Language))
        {
            return BadRequest(new { message = "Language is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return BadRequest(new { message = "Currency is required." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.UserPreference)
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        if (userProfile.UserPreference is null)
        {
            userProfile.UserPreference = new UserPreference
            {
                Id = Guid.NewGuid(),
                UserProfileId = userProfile.Id
            };

            dbContext.UserPreferences.Add(userProfile.UserPreference);
        }

        userProfile.UserPreference.Language = request.Language;
        userProfile.UserPreference.Currency = request.Currency;
        userProfile.UserPreference.NotificationEmail = request.NotificationEmail;
        userProfile.UserPreference.NotificationSms = request.NotificationSms;
        userProfile.UserPreference.NotificationPush = request.NotificationPush;

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(userProfile.UserPreference));
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private static UserPreferenceResponse ToResponse(UserPreference userPreference)
    {
        return new UserPreferenceResponse(
            userPreference.Id,
            userPreference.Language,
            userPreference.Currency,
            userPreference.NotificationEmail,
            userPreference.NotificationSms,
            userPreference.NotificationPush);
    }
}
