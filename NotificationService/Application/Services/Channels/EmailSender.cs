using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services.Channels;

/// <summary>
/// Development email transport. A real deployment would swap this for an SMTP /
/// provider-backed implementation; the contract and dispatcher logic stay the same.
/// </summary>
public class EmailSender(ILogger<EmailSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public Task<ChannelResult> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notification.Recipient) || !notification.Recipient.Contains('@'))
        {
            return Task.FromResult(ChannelResult.Fail("Recipient is not a valid email address."));
        }

        logger.LogInformation(
            "Email dispatched to {Recipient} (subject: {Subject}).",
            notification.Recipient,
            notification.Subject ?? "(none)");

        return Task.FromResult(ChannelResult.Ok($"Email accepted for {notification.Recipient}."));
    }
}
