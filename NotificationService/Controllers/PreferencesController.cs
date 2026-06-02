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
[Route("api/notifications/preferences")]
public class PreferencesController(
    NotificationDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>Returns the current user's preferences, defaulting all channels on.</summary>
    [HttpGet]
    public async Task<ActionResult<PreferenceResponse>> Get(CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        var preference = await GetOrCreateAsync(user.UserId, cancellationToken);
        return Ok(ToResponse(preference));
    }

    [HttpPut]
    public async Task<ActionResult<PreferenceResponse>> Update(UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        var preference = await GetOrCreateAsync(user.UserId, cancellationToken);

        preference.EmailEnabled = request.EmailEnabled ?? preference.EmailEnabled;
        preference.SmsEnabled = request.SmsEnabled ?? preference.SmsEnabled;
        preference.PushEnabled = request.PushEnabled ?? preference.PushEnabled;
        preference.MarketingOptIn = request.MarketingOptIn ?? preference.MarketingOptIn;
        preference.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(preference));
    }

    private async Task<NotificationPreference> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preference = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(existing => existing.UserId == userId, cancellationToken);

        if (preference is not null)
        {
            return preference;
        }

        preference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true,
            PushEnabled = true,
            MarketingOptIn = false,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.NotificationPreferences.Add(preference);
        await dbContext.SaveChangesAsync(cancellationToken);

        return preference;
    }

    private static PreferenceResponse ToResponse(NotificationPreference preference)
    {
        return new PreferenceResponse(
            preference.Id,
            preference.UserId,
            preference.EmailEnabled,
            preference.SmsEnabled,
            preference.PushEnabled,
            preference.MarketingOptIn,
            preference.UpdatedAt);
    }
}
