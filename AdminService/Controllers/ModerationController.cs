using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/moderation/cases")]
public class ModerationController(
    IModerationService moderationService,
    ICurrentAdminService currentAdminService) : AdminControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ModerationCaseResponse>> Create(CreateModerationCaseRequest request)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var moderationCase = await moderationService.CreateAsync(request, adminUserId.Value);

        return CreatedAtAction(nameof(GetById), new { id = moderationCase.Id }, ToResponse(moderationCase));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModerationCaseResponse>>> GetAll()
    {
        var moderationCases = await moderationService.GetAllAsync();
        return Ok(moderationCases.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModerationCaseResponse>> GetById(Guid id)
    {
        var moderationCase = await moderationService.GetByIdAsync(id);

        return moderationCase is null
            ? NotFound(new { message = "Moderation case was not found." })
            : Ok(ToResponse(moderationCase));
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<ModerationCaseResponse>> Assign(Guid id, AssignModerationCaseRequest request)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var moderationCase = await moderationService.AssignAsync(id, request, adminUserId.Value);

        return moderationCase is null
            ? NotFound(new { message = "Moderation case was not found." })
            : Ok(ToResponse(moderationCase));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ModerationCaseResponse>> Approve(Guid id, ModerationDecisionRequest request)
    {
        return await CompleteAsync(id, request, moderationService.ApproveAsync);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ModerationCaseResponse>> Reject(Guid id, ModerationDecisionRequest request)
    {
        return await CompleteAsync(id, request, moderationService.RejectAsync);
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<ModerationCaseResponse>> Resolve(Guid id, ModerationDecisionRequest request)
    {
        return await CompleteAsync(id, request, moderationService.ResolveAsync);
    }

    private async Task<ActionResult<ModerationCaseResponse>> CompleteAsync(
        Guid id,
        ModerationDecisionRequest request,
        Func<Guid, ModerationDecisionRequest, Guid, Task<ModerationCase?>> complete)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var moderationCase = await complete(id, request, adminUserId.Value);

        return moderationCase is null
            ? NotFound(new { message = "Moderation case was not found." })
            : Ok(ToResponse(moderationCase));
    }

    private static ModerationCaseResponse ToResponse(ModerationCase moderationCase)
    {
        return new ModerationCaseResponse(
            moderationCase.Id,
            moderationCase.ReportedEntityType,
            moderationCase.ReportedEntityId,
            moderationCase.ReportedByUserId,
            moderationCase.AssignedAdminId,
            moderationCase.Status,
            moderationCase.Reason,
            moderationCase.Decision,
            moderationCase.CreatedAt,
            moderationCase.ResolvedAt);
    }
}
