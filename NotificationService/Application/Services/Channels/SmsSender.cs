using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services.Channels;

/// <summary>Development SMS transport (stand-in for a Twilio/Vonage-backed sender).</summary>
public class SmsSender(ILogger<SmsSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public Task<ChannelResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient))
        {
            return Task.FromResult(ChannelResult.Fail("Recipient phone number is required."));
        }

        logger.LogInformation("SMS dispatched to {Recipient}.", notification.Recipient);

        return Task.FromResult(ChannelResult.Ok($"SMS accepted for {notification.Recipient}."));
    }
}
