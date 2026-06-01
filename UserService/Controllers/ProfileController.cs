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
[Route("api/users")]
public class ProfileController(UserDbContext dbContext, IUserActivityService userActivityService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCompleteProfileQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        return userProfile is null
            ? NotFound(new { message = "User profile was not found." })
            : Ok(ToResponse(userProfile));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfileResponse>> GetById(Guid id)
    {
        var userProfile = await GetCompleteProfileQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.Id == id);

        return userProfile is null
            ? NotFound(new { message = "User profile was not found." })
            : Ok(ToResponse(userProfile));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(UpdateProfileRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCompleteProfileQuery()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        userProfile.FirstName = request.FirstName ?? userProfile.FirstName;
        userProfile.LastName = request.LastName ?? userProfile.LastName;
        userProfile.BirthDate = request.BirthDate ?? userProfile.BirthDate;
        userProfile.PhoneNumber = request.PhoneNumber ?? userProfile.PhoneNumber;
        userProfile.Address = request.Address ?? userProfile.Address;
        userProfile.Country = request.Country ?? userProfile.Country;
        userProfile.City = request.City ?? userProfile.City;
        userProfile.ProfilePictureUrl = request.ProfilePictureUrl ?? userProfile.ProfilePictureUrl;
        userProfile.Bio = request.Bio ?? userProfile.Bio;
        userProfile.UpdatedAt = DateTime.UtcNow;

        userActivityService.Log(
            userProfile,
            UserActivityService.ProfileUpdated,
            "User profile was updated.");

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(userProfile));
    }

    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        dbContext.UserProfiles.Remove(userProfile);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private IQueryable<UserProfile> GetCompleteProfileQuery()
    {
        return dbContext.UserProfiles
            .Include(profile => profile.UserPacks)
            .Include(profile => profile.IdentityVerifications)
            .Include(profile => profile.UserDocuments)
            .Include(profile => profile.UserPreference)
            .Include(profile => profile.UserActivities)
            .Include(profile => profile.TrustScore);
    }

    private static UserProfileResponse ToResponse(UserProfile userProfile)
    {
        return new UserProfileResponse(
            userProfile.Id,
            userProfile.AuthUserId,
            userProfile.FirstName,
            userProfile.LastName,
            userProfile.BirthDate,
            userProfile.PhoneNumber,
            userProfile.Address,
            userProfile.Country,
            userProfile.City,
            userProfile.ProfilePictureUrl,
            userProfile.Bio,
            userProfile.Status,
            userProfile.CreatedAt,
            userProfile.UpdatedAt,
            userProfile.IsDeleted,
            userProfile.DeletedAt,
            ToUserPackResponse(GetCurrentPack(userProfile.UserPacks)),
            userProfile.IdentityVerifications
                .Select(identityVerification => new IdentityVerificationResponse(
                    identityVerification.Id,
                    identityVerification.VerificationStatus,
                    identityVerification.VerifiedAt,
                    identityVerification.RejectionReason))
                .ToList(),
            userProfile.UserDocuments
                .Select(userDocument => new UserDocumentResponse(
                    userDocument.Id,
                    userDocument.DocumentType,
                    userDocument.FileUrl,
                    userDocument.UploadedAt,
                    userDocument.Status))
                .ToList(),
            userProfile.UserPreference is null
                ? null
                : new UserPreferenceResponse(
                    userProfile.UserPreference.Id,
                    userProfile.UserPreference.Language,
                    userProfile.UserPreference.Currency,
                    userProfile.UserPreference.NotificationEmail,
                    userProfile.UserPreference.NotificationSms,
                    userProfile.UserPreference.NotificationPush),
            userProfile.UserActivities
                .Select(userActivity => new UserActivityResponse(
                    userActivity.Id,
                    userActivity.Action,
                    userActivity.Description,
                    userActivity.CreatedAt))
                .ToList(),
            userProfile.TrustScore is null
                ? null
                : new TrustScoreResponse(
                    userProfile.TrustScore.Id,
                    userProfile.TrustScore.Score,
                    userProfile.TrustScore.LastCalculatedAt));
    }

    private static UserPack? GetCurrentPack(IEnumerable<UserPack> userPacks)
    {
        var now = DateTime.UtcNow;

        return userPacks
            .Where(userPack => userPack.IsActive
                && userPack.StartDate <= now
                && (userPack.EndDate is null || userPack.EndDate >= now))
            .OrderByDescending(userPack => userPack.StartDate)
            .FirstOrDefault();
    }

    private static UserPackResponse? ToUserPackResponse(UserPack? userPack)
    {
        return userPack is null
            ? null
            : new UserPackResponse(
                userPack.Id,
                userPack.PackType,
                userPack.StartDate,
                userPack.EndDate,
                userPack.IsActive);
    }
}
