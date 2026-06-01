using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/users/documents")]
public class DocumentController(UserDbContext dbContext, IUserActivityService userActivityService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDocumentResponse>> Create(CreateUserDocumentRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.FileUrl))
        {
            return BadRequest(new { message = "FileUrl is required." });
        }

        var userProfile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var userDocument = new UserDocument
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            DocumentType = request.DocumentType,
            FileUrl = request.FileUrl,
            UploadedAt = DateTime.UtcNow,
            Status = VerificationStatus.Pending
        };

        dbContext.UserDocuments.Add(userDocument);
        userActivityService.Log(
            userProfile,
            UserActivityService.DocumentUploaded,
            $"Document uploaded: {request.DocumentType}.");

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(userDocument));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDocumentResponse>>> GetDocuments()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.UserDocuments)
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var documents = userProfile.UserDocuments
            .OrderByDescending(document => document.UploadedAt)
            .Select(ToResponse)
            .ToList();

        return Ok(documents);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDocumentResponse>> GetById(Guid id)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userDocument = await GetUserDocumentAsync(id, authUserId.Value, asNoTracking: true);
        return userDocument is null
            ? NotFound(new { message = "User document was not found." })
            : Ok(ToResponse(userDocument));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var userDocument = await GetUserDocumentAsync(id, authUserId.Value, asNoTracking: false);
        if (userDocument is null)
        {
            return NotFound(new { message = "User document was not found." });
        }

        dbContext.UserDocuments.Remove(userDocument);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private async Task<UserDocument?> GetUserDocumentAsync(Guid documentId, Guid authUserId, bool asNoTracking)
    {
        var query = dbContext.UserDocuments
            .Include(document => document.UserProfile)
            .Where(document => document.Id == documentId && document.UserProfile.AuthUserId == authUserId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    private static UserDocumentResponse ToResponse(UserDocument userDocument)
    {
        return new UserDocumentResponse(
            userDocument.Id,
            userDocument.DocumentType,
            userDocument.FileUrl,
            userDocument.UploadedAt,
            userDocument.Status);
    }
}
