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
[Route("api/users/verification")]
public class IdentityVerificationController(UserDbContext dbContext) : ControllerBase
{
    [HttpPost("request")]
    public async Task<ActionResult<IdentityVerificationResponse>> RequestVerification()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.IdentityVerifications)
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        if (userProfile.IdentityVerifications.Any(IsPending))
        {
            return BadRequest(new { message = "A verification request is already pending." });
        }

        if (userProfile.IdentityVerifications.Any(IsApproved))
        {
            return BadRequest(new { message = "User profile is already verified." });
        }

        var verification = new IdentityVerification
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            VerificationStatus = VerificationStatus.Pending
        };

        dbContext.IdentityVerifications.Add(verification);
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(verification));
    }

    [HttpGet("status")]
    public async Task<ActionResult<IdentityVerificationResponse>> GetStatus()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.IdentityVerifications)
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var verification = GetCurrentVerification(userProfile.IdentityVerifications);
        return verification is null
            ? NotFound(new { message = "No identity verification request was found." })
            : Ok(ToResponse(verification));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("approve")]
    public async Task<ActionResult<IdentityVerificationResponse>> Approve(VerificationDecisionRequest request)
    {
        var verification = await GetPendingVerificationAsync(request.UserProfileId);
        if (verification is null)
        {
            return NotFound(new { message = "No pending verification request was found." });
        }

        verification.VerificationStatus = VerificationStatus.Approved;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.RejectionReason = null;

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(verification));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reject")]
    public async Task<ActionResult<IdentityVerificationResponse>> Reject(VerificationDecisionRequest request)
    {
        var verification = await GetPendingVerificationAsync(request.UserProfileId);
        if (verification is null)
        {
            return NotFound(new { message = "No pending verification request was found." });
        }

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            return BadRequest(new { message = "Rejection reason is required." });
        }

        verification.VerificationStatus = VerificationStatus.Rejected;
        verification.VerifiedAt = null;
        verification.RejectionReason = request.RejectionReason;

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(verification));
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private async Task<IdentityVerification?> GetPendingVerificationAsync(Guid userProfileId)
    {
        return await dbContext.IdentityVerifications
            .FirstOrDefaultAsync(verification =>
                verification.UserProfileId == userProfileId
                && verification.VerificationStatus == VerificationStatus.Pending);
    }

    private static IdentityVerification? GetCurrentVerification(IEnumerable<IdentityVerification> verifications)
    {
        return verifications.FirstOrDefault(IsPending)
            ?? verifications.FirstOrDefault(IsApproved)
            ?? verifications.FirstOrDefault(verification => verification.VerificationStatus == VerificationStatus.Rejected);
    }

    private static bool IsPending(IdentityVerification verification)
    {
        return verification.VerificationStatus == VerificationStatus.Pending;
    }

    private static bool IsApproved(IdentityVerification verification)
    {
        return verification.VerificationStatus == VerificationStatus.Approved;
    }

    private static IdentityVerificationResponse ToResponse(IdentityVerification verification)
    {
        return new IdentityVerificationResponse(
            verification.Id,
            verification.VerificationStatus,
            verification.VerifiedAt,
            verification.RejectionReason);
    }
}
