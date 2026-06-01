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
[Route("api/users/company")]
public class CompanyController(UserDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompanyAccountResponse>> Create(CreateCompanyAccountRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return BadRequest(new { message = "CompanyName is required." });
        }

        var userProfileExists = await dbContext.UserProfiles
            .AnyAsync(profile => profile.AuthUserId == authUserId.Value);

        if (!userProfileExists)
        {
            return NotFound(new { message = "User profile was not found." });
        }

        var companyAccount = new CompanyAccount
        {
            Id = Guid.NewGuid(),
            OwnerUserId = authUserId.Value,
            CompanyName = request.CompanyName,
            RegistrationNumber = request.RegistrationNumber,
            TaxNumber = request.TaxNumber,
            CreatedAt = DateTime.UtcNow
        };

        companyAccount.Members.Add(new CompanyMember
        {
            Id = Guid.NewGuid(),
            CompanyAccountId = companyAccount.Id,
            UserId = authUserId.Value,
            Role = "Owner"
        });

        dbContext.CompanyAccounts.Add(companyAccount);
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(companyAccount));
    }

    [HttpPost("members")]
    public async Task<ActionResult<CompanyMemberResponse>> AddMember(AddCompanyMemberRequest request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new { message = "Role is required." });
        }

        var companyAccount = await dbContext.CompanyAccounts
            .Include(companyAccount => companyAccount.Members)
            .FirstOrDefaultAsync(companyAccount => companyAccount.Id == request.CompanyAccountId);

        if (companyAccount is null)
        {
            return NotFound(new { message = "Company account was not found." });
        }

        if (companyAccount.OwnerUserId != authUserId.Value)
        {
            return Forbid();
        }

        var userProfileExists = await dbContext.UserProfiles
            .AnyAsync(profile => profile.AuthUserId == request.UserId);

        if (!userProfileExists)
        {
            return NotFound(new { message = "Member user profile was not found." });
        }

        if (companyAccount.Members.Any(member => member.UserId == request.UserId))
        {
            return BadRequest(new { message = "User is already a company member." });
        }

        var companyMember = new CompanyMember
        {
            Id = Guid.NewGuid(),
            CompanyAccountId = companyAccount.Id,
            UserId = request.UserId,
            Role = request.Role
        };

        dbContext.CompanyMembers.Add(companyMember);
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(companyMember));
    }

    [HttpDelete("members/{id:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id)
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var companyMember = await dbContext.CompanyMembers
            .Include(member => member.CompanyAccount)
            .FirstOrDefaultAsync(member => member.Id == id);

        if (companyMember is null)
        {
            return NotFound(new { message = "Company member was not found." });
        }

        if (companyMember.CompanyAccount.OwnerUserId != authUserId.Value)
        {
            return Forbid();
        }

        if (companyMember.UserId == companyMember.CompanyAccount.OwnerUserId)
        {
            return BadRequest(new { message = "Company owner cannot be removed from members." });
        }

        dbContext.CompanyMembers.Remove(companyMember);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompanyAccountResponse>>> GetCompanies()
    {
        var authUserId = GetAuthUserId();
        if (authUserId is null)
        {
            return Unauthorized(new { message = "User id claim is missing or invalid." });
        }

        var companyAccounts = await dbContext.CompanyAccounts
            .Include(companyAccount => companyAccount.Members)
            .AsNoTracking()
            .Where(companyAccount =>
                companyAccount.OwnerUserId == authUserId.Value
                || companyAccount.Members.Any(member => member.UserId == authUserId.Value))
            .OrderByDescending(companyAccount => companyAccount.CreatedAt)
            .ToListAsync();

        return Ok(companyAccounts.Select(ToResponse).ToList());
    }

    private Guid? GetAuthUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var authUserId) ? authUserId : null;
    }

    private static CompanyAccountResponse ToResponse(CompanyAccount companyAccount)
    {
        return new CompanyAccountResponse(
            companyAccount.Id,
            companyAccount.OwnerUserId,
            companyAccount.CompanyName,
            companyAccount.RegistrationNumber,
            companyAccount.TaxNumber,
            companyAccount.CreatedAt,
            companyAccount.Members.Select(ToResponse).ToList());
    }

    private static CompanyMemberResponse ToResponse(CompanyMember companyMember)
    {
        return new CompanyMemberResponse(
            companyMember.Id,
            companyMember.CompanyAccountId,
            companyMember.UserId,
            companyMember.Role);
    }
}
