using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Application.Services;

public class NotificationDispatcher(
    NotificationDbContext dbContext,
    IEnumerable<INotificationChannelSender> senders,
    IWebhookPublisher webhookPublisher,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    public async Task<Notification> DispatchAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        // Already delivered or explicitly cancelled — nothing to do.
        if (notification.Status is NotificationStatus.Sent or NotificationStatus.Cancelled)
        {
            return notification;
        }

        if (await IsChannelDisabledAsync(notification, cancellationToken))
        {
            notification.Status = NotificationStatus.Cancelled;
            notification.LastError = "Recipient has disabled this channel.";
            notification.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return notification;
        }

        if (notification.AttemptCount >= notification.MaxAttempts)
        {
            notification.Status = NotificationStatus.Failed;
            notification.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return notification;
        }

        var sender = senders.FirstOrDefault(candidate => candidate.Channel == notification.Channel);
        notification.AttemptCount += 1;

        ChannelResult result;
        if (sender is null)
        {
            result = ChannelResult.Fail($"No sender registered for channel '{notification.Channel}'.");
        }
        else
        {
            try
            {
                result = await sender.SendAsync(notification, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Channel {Channel} threw while sending notification {Id}.", notification.Channel, notification.Id);
                result = ChannelResult.Fail(exception.Message);
            }
        }

        var now = DateTime.UtcNow;
        dbContext.NotificationAttempts.Add(new NotificationAttempt
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            AttemptNumber = notification.AttemptCount,
            Status = result.Success ? AttemptStatus.Succeeded : AttemptStatus.Failed,
            Detail = result.Detail,
            AttemptedAt = now
        });

        if (result.Success)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = now;
            notification.LastError = null;
        }
        else
        {
            // Stay Pending while retries remain so the worker picks it up again.
            notification.Status = notification.AttemptCount >= notification.MaxAttempts
                ? NotificationStatus.Failed
                : NotificationStatus.Pending;
            notification.LastError = result.Detail;
        }

        notification.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await PublishOutcomeAsync(notification, result.Success, cancellationToken);

        return notification;
    }

    private async Task<bool> IsChannelDisabledAsync(Notification notification, CancellationToken cancellationToken)
    {
        var preference = await dbContext.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == notification.RecipientUserId, cancellationToken);

        if (preference is null)
        {
            return false;
        }

        return notification.Channel switch
        {
            NotificationChannel.Email => !preference.EmailEnabled,
            NotificationChannel.Sms => !preference.SmsEnabled,
            NotificationChannel.Push => !preference.PushEnabled,
            _ => false
        };
    }

    private async Task PublishOutcomeAsync(Notification notification, bool success, CancellationToken cancellationToken)
    {
        var payload = new
        {
            notification.Id,
            notification.RecipientUserId,
            Channel = notification.Channel.ToString(),
            Status = notification.Status.ToString(),
            notification.Subject,
            notification.AttemptCount,
            OccurredAt = DateTime.UtcNow
        };

        var eventType = success ? WebhookEventType.NotificationSent : WebhookEventType.NotificationFailed;

        try
        {
            await webhookPublisher.PublishAsync(eventType, payload, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to publish {EventType} webhook for notification {Id}.", eventType, notification.Id);
        }
    }
}
