using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/packs")]
public class PackController(UserDbContext dbContext, IUserActivityService userActivityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserPackResponse>>> GetPacks()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCurrentUserProfileWithPacksAsync(authUserId.Value);
        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var hasChanges = NormalizePackHistory(userProfile.UserPacks, DateTime.UtcNow);
        if (hasChanges)
        {
            await dbContext.SaveChangesAsync();
        }

        var packs = userProfile.UserPacks
            .OrderByDescending(userPack => userPack.StartDate)
            .Select(ToResponse)
            .ToList();

        return Ok(packs);
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<UserPackResponse>> Subscribe(SubscribePackRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCurrentUserProfileWithPacksAsync(authUserId.Value);
        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var now = DateTime.UtcNow;
        var startDate = request.StartDate ?? now;

        if (startDate > now)
        {
            return BadRequest(new { message = "Start date must be now or in the past." });
        }

        if (request.EndDate is not null && request.EndDate <= startDate)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        NormalizePackHistory(userProfile.UserPacks, now);

        var currentPack = userProfile.UserPacks.FirstOrDefault(userPack => IsCurrentlyValid(userPack, now));
        if (currentPack is not null)
        {
            return BadRequest(new { message = "User already has an active valid pack." });
        }

        var userPack = new UserPack
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            PackType = request.PackType,
            StartDate = startDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        dbContext.UserPacks.Add(userPack);
        userActivityService.Log(
            userProfile,
            UserActivityService.PackChanged,
            $"Subscribed to {request.PackType} pack.");

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(userPack));
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCurrentUserProfileWithPacksAsync(authUserId.Value);
        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var now = DateTime.UtcNow;
        NormalizePackHistory(userProfile.UserPacks, now);

        var currentPack = userProfile.UserPacks.FirstOrDefault(userPack => IsCurrentlyValid(userPack, now));
        if (currentPack is null)
        {
            await dbContext.SaveChangesAsync();
            return NotFound(new { message = "No active valid pack was found." });
        }

        currentPack.IsActive = false;
        currentPack.EndDate = now;

        userActivityService.Log(
            userProfile,
            UserActivityService.PackChanged,
            $"Canceled {currentPack.PackType} pack.");

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserPackResponse>> GetCurrent()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCurrentUserProfileWithPacksAsync(authUserId.Value);
        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var now = DateTime.UtcNow;
        var hasChanges = NormalizePackHistory(userProfile.UserPacks, now);

        var currentPack = userProfile.UserPacks
            .Where(userPack => IsCurrentlyValid(userPack, now))
            .OrderByDescending(userPack => userPack.StartDate)
            .FirstOrDefault();

        if (currentPack is null)
        {
            if (hasChanges)
            {
                await dbContext.SaveChangesAsync();
            }

            return NotFound(new { message = "No active valid pack was found." });
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync();
        }

        return Ok(ToResponse(currentPack));
    }

    private async Task<UserProfile?> GetCurrentUserProfileWithPacksAsync(Guid authUserId)
    {
        return await dbContext.UserProfiles
            .Include(userProfile => userProfile.UserPacks)
            .FirstOrDefaultAsync(userProfile => userProfile.AuthUserId == authUserId);
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private static bool NormalizePackHistory(ICollection<UserPack> userPacks, DateTime now)
    {
        var hasChanges = false;

        foreach (var userPack in userPacks.Where(userPack =>
            userPack.IsActive && userPack.EndDate is { } endDate && endDate < now))
        {
            userPack.IsActive = false;
            hasChanges = true;
        }

        var activeValidPacks = userPacks
            .Where(userPack => IsCurrentlyValid(userPack, now))
            .OrderByDescending(userPack => userPack.StartDate)
            .ToList();

        foreach (var userPack in activeValidPacks.Skip(1))
        {
            userPack.IsActive = false;
            if (userPack.EndDate is null || userPack.EndDate > now)
            {
                userPack.EndDate = now;
            }

            hasChanges = true;
        }

        return hasChanges;
    }

    private static bool IsCurrentlyValid(UserPack userPack, DateTime now)
    {
        return userPack.IsActive
            && userPack.StartDate <= now
            && (userPack.EndDate is null || userPack.EndDate >= now);
    }

    private static UserPackResponse ToResponse(UserPack userPack)
    {
        return new UserPackResponse(
            userPack.Id,
            userPack.PackType,
            userPack.StartDate,
            userPack.EndDate,
            userPack.IsActive);
    }
}
