using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/content")]
public class AdminContentController(
    IContentServiceClient contentServiceClient,
    ICurrentAdminService currentAdminService,
    AdminDbContext dbContext) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetContent(CancellationToken cancellationToken)
    {
        try
        {
            var content = await contentServiceClient.GetContentAsync(cancellationToken);
            return Ok(content);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContentById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var content = await contentServiceClient.GetContentByIdAsync(id, cancellationToken);

            return content is null
                ? NotFound(new { message = "Content was not found." })
                : Ok(content);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteContentActionAsync(
            id,
            cancellationToken,
            contentServiceClient.ApproveContentAsync,
            "ApproveContent",
            "Content approved successfully.");
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteContentActionAsync(
            id,
            cancellationToken,
            contentServiceClient.RejectContentAsync,
            "RejectContent",
            "Content rejected successfully.");
    }

    [HttpPost("{id:guid}/remove")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteContentActionAsync(
            id,
            cancellationToken,
            contentServiceClient.RemoveContentAsync,
            "RemoveContent",
            "Content removed successfully.");
    }

    private async Task<IActionResult> ExecuteContentActionAsync(
        Guid id,
        CancellationToken cancellationToken,
        Func<Guid, Guid, CancellationToken, Task> contentAction,
        string action,
        string successMessage)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        await AddActionLogAsync(adminUserId.Value, action, id, cancellationToken);

        try
        {
            await contentAction(id, adminUserId.Value, cancellationToken);
            return Ok(new { message = successMessage });
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    private async Task AddActionLogAsync(Guid adminUserId, string action, Guid contentId, CancellationToken cancellationToken)
    {
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            TargetService = "Content",
            TargetEntity = "Content",
            TargetEntityId = contentId,
            Description = $"{action} requested for content {contentId}.",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private ObjectResult NotImplemented(string message)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message,
            targetServices = new[] { "VehicleService", "MediaService" }
        });
    }
}
