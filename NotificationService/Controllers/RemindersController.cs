using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications/reminders")]
public class RemindersController(
    NotificationDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderResponse>>> List(CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        var reminders = await dbContext.Reminders
            .AsNoTracking()
            .Where(reminder => reminder.UserId == user.UserId)
            .OrderBy(reminder => reminder.RemindAt)
            .ToListAsync(cancellationToken);

        return Ok(reminders.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ReminderResponse>> Create(CreateReminderRequest request, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Reminder title is required." });
        }

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            Title = request.Title,
            Message = request.Message,
            Channel = request.Channel,
            Recipient = request.Recipient,
            RemindAt = DateTime.SpecifyKind(request.RemindAt, DateTimeKind.Utc),
            Recurrence = request.Recurrence ?? RecurrenceType.None,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Reminders.Add(reminder);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = reminder.Id }, ToResponse(reminder));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReminderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var reminder = await FindOwnedAsync(id, cancellationToken);
        return reminder is null
            ? NotFound(new { message = "Reminder was not found." })
            : Ok(ToResponse(reminder));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReminderResponse>> Update(Guid id, UpdateReminderRequest request, CancellationToken cancellationToken)
    {
        var reminder = await FindOwnedAsync(id, cancellationToken);
        if (reminder is null)
        {
            return NotFound(new { message = "Reminder was not found." });
        }

        reminder.Title = request.Title ?? reminder.Title;
        reminder.Message = request.Message ?? reminder.Message;
        reminder.Channel = request.Channel ?? reminder.Channel;
        reminder.Recipient = request.Recipient ?? reminder.Recipient;
        reminder.Recurrence = request.Recurrence ?? reminder.Recurrence;
        reminder.IsActive = request.IsActive ?? reminder.IsActive;

        if (request.RemindAt is { } remindAt)
        {
            reminder.RemindAt = DateTime.SpecifyKind(remindAt, DateTimeKind.Utc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(reminder));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var reminder = await FindOwnedAsync(id, cancellationToken);
        if (reminder is null)
        {
            return NotFound(new { message = "Reminder was not found." });
        }

        dbContext.Reminders.Remove(reminder);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<Reminder?> FindOwnedAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return null;
        }

        return await dbContext.Reminders
            .FirstOrDefaultAsync(reminder => reminder.Id == id && reminder.UserId == user.UserId, cancellationToken);
    }

    private static ReminderResponse ToResponse(Reminder reminder)
    {
        return new ReminderResponse(
            reminder.Id,
            reminder.UserId,
            reminder.Title,
            reminder.Message,
            reminder.Channel,
            reminder.Recipient,
            reminder.RemindAt,
            reminder.Recurrence,
            reminder.IsActive,
            reminder.LastTriggeredAt,
            reminder.CreatedAt);
    }
}
