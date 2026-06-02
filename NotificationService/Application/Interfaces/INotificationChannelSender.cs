using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

/// <summary>Outcome of a single channel delivery attempt.</summary>
public record ChannelResult(bool Success, string Detail)
{
    public static ChannelResult Ok(string detail) => new(true, detail);

    public static ChannelResult Fail(string detail) => new(false, detail);
}

/// <summary>
/// A transport for one channel (email, SMS, push). Implementations are registered
/// per channel and selected by the dispatcher via <see cref="Channel"/>.
/// </summary>
public interface INotificationChannelSender
{
    NotificationChannel Channel { get; }

    Task<ChannelResult> SendAsync(Notification notification, CancellationToken cancellationToken = default);
}
