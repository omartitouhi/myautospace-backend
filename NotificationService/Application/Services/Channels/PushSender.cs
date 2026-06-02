using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services.Channels;

/// <summary>Development push transport (stand-in for an FCM/APNs-backed sender).</summary>
public class PushSender(ILogger<PushSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Push;

    public Task<ChannelResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            return Task.FromResult(ChannelResult.Fail("Recipient device token is required."));
        }

        logger.LogInformation("Push notification dispatched to device {Recipient}.", notification.Recipient);

        return Task.FromResult(ChannelResult.Ok($"Push accepted for device {notification.Recipient}."));
    }
}
