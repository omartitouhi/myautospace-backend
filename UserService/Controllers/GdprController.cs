using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/gdpr")]
public class GdprController(UserDbContext dbContext) : ControllerBase
{
    [HttpGet("export")]
    public async Task<ActionResult<GdprExportResponse>> Export()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await GetCompleteProfileQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId.Value);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var companies = await dbContext.CompanyAccounts
            .Include(companyAccount => companyAccount.Members)
            .AsNoTracking()
            .Where(companyAccount =>
                companyAccount.OwnerUserId == authUserId.Value
                || companyAccount.Members.Any(member => member.UserId == authUserId.Value))
            .OrderByDescending(companyAccount => companyAccount.CreatedAt)
            .ToListAsync();

        AddGdprAuditLog(userProfile.Id, "Export", "User data export was requested.");
        await dbContext.SaveChangesAsync();

        var gdprHistory = await dbContext.GdprAuditLogs
            .AsNoTracking()
            .Where(log => log.UserProfileId == userProfile.Id)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();

        return Ok(new GdprExportResponse(
            DateTime.UtcNow,
            ToUserProfileResponse(userProfile),
            companies.Select(ToCompanyAccountResponse).ToList(),
            gdprHistory.Select(ToGdprAuditLogResponse).ToList()));
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.UserPacks)
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId.Value);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        if (userProfile.IsDeleted)
        {
            return NoContent();
        }

        var now = DateTime.UtcNow;
        userProfile.IsDeleted = true;
        userProfile.DeletedAt = now;
        userProfile.Status = UserStatus.Blocked;
        userProfile.UpdatedAt = now;

        foreach (var userPack in userProfile.UserPacks.Where(userPack => userPack.IsActive))
        {
            userPack.IsActive = false;
            userPack.EndDate ??= now;
        }

        AddGdprAuditLog(userProfile.Id, "DeleteAccount", "User account was logically deleted.");

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
            .Include(profile => profile.TrustScore)
            .Include(profile => profile.GdprAuditLogs);
    }

    private void AddGdprAuditLog(Guid userProfileId, string action, string description)
    {
        dbContext.GdprAuditLogs.Add(new GdprAuditLog
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static UserProfileResponse ToUserProfileResponse(UserProfile userProfile)
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
                .Select(verification => new IdentityVerificationResponse(
                    verification.Id,
                    verification.VerificationStatus,
                    verification.VerifiedAt,
                    verification.RejectionReason))
                .ToList(),
            userProfile.UserDocuments
                .Select(document => new UserDocumentResponse(
                    document.Id,
                    document.DocumentType,
                    document.FileUrl,
                    document.UploadedAt,
                    document.Status))
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
                .Select(activity => new UserActivityResponse(
                    activity.Id,
                    activity.Action,
                    activity.Description,
                    activity.CreatedAt))
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

    private static CompanyAccountResponse ToCompanyAccountResponse(CompanyAccount companyAccount)
    {
        return new CompanyAccountResponse(
            companyAccount.Id,
            companyAccount.OwnerUserId,
            companyAccount.CompanyName,
            companyAccount.RegistrationNumber,
            companyAccount.TaxNumber,
            companyAccount.CreatedAt,
            companyAccount.Members.Select(ToCompanyMemberResponse).ToList());
    }

    private static CompanyMemberResponse ToCompanyMemberResponse(CompanyMember companyMember)
    {
        return new CompanyMemberResponse(
            companyMember.Id,
            companyMember.CompanyAccountId,
            companyMember.UserId,
            companyMember.Role);
    }

    private static GdprAuditLogResponse ToGdprAuditLogResponse(GdprAuditLog gdprAuditLog)
    {
        return new GdprAuditLogResponse(
            gdprAuditLog.Id,
            gdprAuditLog.Action,
            gdprAuditLog.Description,
            gdprAuditLog.CreatedAt);
    }
}
