using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

public interface IWebhookPublisher
{
    /// <summary>
    /// Delivers a signed JSON payload to every active webhook subscribed to the event.
    /// Each delivery is recorded; failures are isolated per subscription.
    /// </summary>
    Task PublishAsync(WebhookEventType eventType, object payload, CancellationToken cancellationToken = default);
}
