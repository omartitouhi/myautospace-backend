using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Constants;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(
    NotificationDbContext dbContext,
    INotificationDispatcher dispatcher,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>Queue a notification. Delivered immediately unless scheduled in the future.</summary>
    [HttpPost]
    [Authorize(Policy = NotificationPolicies.Sender)]
    public async Task<ActionResult<NotificationResponse>> Send(SendNotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { message = "Notification body is required." });
        }

        var now = DateTime.UtcNow;
        var scheduledAt = request.ScheduledAt is { } value ? ToUtc(value) : (DateTime?)null;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = request.RecipientUserId,
            Channel = request.Channel,
            Recipient = request.Recipient,
            Subject = request.Subject,
            Body = request.Body,
            Status = NotificationStatus.Pending,
            MaxAttempts = Math.Clamp(request.MaxAttempts ?? 3, 1, 10),
            ScheduledAt = scheduledAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Future-dated notifications are left for the background worker.
        if (scheduledAt is null || scheduledAt <= now)
        {
            await dispatcher.DispatchAsync(notification, cancellationToken);
        }

        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, ToResponse(notification));
    }

    /// <summary>History of the current user's notifications.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> List(
        [FromQuery] NotificationStatus? status,
        CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == user.UserId);

        if (status is { } value)
        {
            query = query.Where(notification => notification.Status == value);
        }

        var notifications = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        return Ok(notifications.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (notification is null)
        {
            return NotFound(new { message = "Notification was not found." });
        }

        return CanAccess(notification.RecipientUserId)
            ? Ok(ToResponse(notification))
            : Forbid();
    }

    /// <summary>The delivery attempt log (history) for a notification.</summary>
    [HttpGet("{id:guid}/attempts")]
    public async Task<ActionResult<IReadOnlyList<NotificationAttemptResponse>>> Attempts(Guid id, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (notification is null)
        {
            return NotFound(new { message = "Notification was not found." });
        }

        if (!CanAccess(notification.RecipientUserId))
        {
            return Forbid();
        }

        var attempts = await dbContext.NotificationAttempts
            .AsNoTracking()
            .Where(attempt => attempt.NotificationId == id)
            .OrderBy(attempt => attempt.AttemptNumber)
            .Select(attempt => new NotificationAttemptResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.Status,
                attempt.Detail,
                attempt.AttemptedAt))
            .ToListAsync(cancellationToken);

        return Ok(attempts);
    }

    /// <summary>Force an immediate redelivery of a failed/pending notification.</summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<NotificationResponse>> Retry(Guid id, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (notification is null)
        {
            return NotFound(new { message = "Notification was not found." });
        }

        if (!CanAccess(notification.RecipientUserId))
        {
            return Forbid();
        }

        if (notification.Status == NotificationStatus.Sent)
        {
            return BadRequest(new { message = "Notification has already been delivered." });
        }

        if (notification.AttemptCount >= notification.MaxAttempts)
        {
            return BadRequest(new { message = "Notification has exhausted its retry budget." });
        }

        // Re-open a cancelled notification for one more try.
        if (notification.Status == NotificationStatus.Cancelled)
        {
            notification.Status = NotificationStatus.Pending;
        }

        await dispatcher.DispatchAsync(notification, cancellationToken);

        return Ok(ToResponse(notification));
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private bool CanAccess(Guid recipientUserId)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return false;
        }

        return user.UserId == recipientUserId || user.Roles.Contains(NotificationRoles.Admin);
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.RecipientUserId,
            notification.Channel,
            notification.Recipient,
            notification.Subject,
            notification.Body,
            notification.Status,
            notification.AttemptCount,
            notification.MaxAttempts,
            notification.ScheduledAt,
            notification.SentAt,
            notification.LastError,
            notification.CreatedAt);
    }
}
