using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications/webhooks")]
public class WebhooksController(
    NotificationDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookResponse>>> List(CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        var webhooks = await dbContext.Webhooks
            .AsNoTracking()
            .Where(webhook => webhook.UserId == user.UserId)
            .OrderByDescending(webhook => webhook.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(webhooks.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<WebhookResponse>> Create(CreateWebhookRequest request, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
        {
            return BadRequest(new { message = "A valid absolute http(s) URL is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Secret))
        {
            return BadRequest(new { message = "A signing secret is required." });
        }

        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Name = request.Name,
            Url = request.Url,
            Secret = request.Secret,
            EventType = request.EventType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Webhooks.Add(webhook);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = webhook.Id }, ToResponse(webhook));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WebhookResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var webhook = await FindOwnedAsync(id, cancellationToken);
        return webhook is null
            ? NotFound(new { message = "Webhook was not found." })
            : Ok(ToResponse(webhook));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WebhookResponse>> Update(Guid id, UpdateWebhookRequest request, CancellationToken cancellationToken)
    {
        var webhook = await FindOwnedAsync(id, cancellationToken);
        if (webhook is null)
        {
            return NotFound(new { message = "Webhook was not found." });
        }

        if (request.Url is not null
            && (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)))
        {
            return BadRequest(new { message = "A valid absolute http(s) URL is required." });
        }

        webhook.Name = request.Name ?? webhook.Name;
        webhook.Url = request.Url ?? webhook.Url;
        webhook.Secret = request.Secret ?? webhook.Secret;
        webhook.EventType = request.EventType ?? webhook.EventType;
        webhook.IsActive = request.IsActive ?? webhook.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(webhook));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var webhook = await FindOwnedAsync(id, cancellationToken);
        if (webhook is null)
        {
            return NotFound(new { message = "Webhook was not found." });
        }

        dbContext.Webhooks.Remove(webhook);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>Delivery history (Historique) for a webhook subscription.</summary>
    [HttpGet("{id:guid}/deliveries")]
    public async Task<ActionResult<IReadOnlyList<WebhookDeliveryResponse>>> Deliveries(Guid id, CancellationToken cancellationToken)
    {
        var webhook = await FindOwnedAsync(id, cancellationToken);
        if (webhook is null)
        {
            return NotFound(new { message = "Webhook was not found." });
        }

        var deliveries = await dbContext.WebhookDeliveries
            .AsNoTracking()
            .Where(delivery => delivery.WebhookId == id)
            .OrderByDescending(delivery => delivery.CreatedAt)
            .Take(200)
            .Select(delivery => new WebhookDeliveryResponse(
                delivery.Id,
                delivery.EventType,
                delivery.Success,
                delivery.ResponseStatusCode,
                delivery.AttemptCount,
                delivery.LastError,
                delivery.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(deliveries);
    }

    private async Task<Webhook?> FindOwnedAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return null;
        }

        return await dbContext.Webhooks
            .FirstOrDefaultAsync(webhook => webhook.Id == id && webhook.UserId == user.UserId, cancellationToken);
    }

    private static WebhookResponse ToResponse(Webhook webhook)
    {
        return new WebhookResponse(
            webhook.Id,
            webhook.UserId,
            webhook.Name,
            webhook.Url,
            webhook.EventType,
            webhook.IsActive,
            webhook.CreatedAt);
    }
}
