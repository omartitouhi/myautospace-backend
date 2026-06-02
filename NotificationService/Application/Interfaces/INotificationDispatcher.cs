using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationDispatcher
{
    /// <summary>
    /// Performs a single delivery attempt for the notification, honouring the
    /// recipient's channel preferences and the notification's retry budget.
    /// Records an attempt, updates status, and publishes the matching webhook event.
    /// </summary>
    Task<Notification> DispatchAsync(Notification notification, CancellationToken cancellationToken = default);
}
