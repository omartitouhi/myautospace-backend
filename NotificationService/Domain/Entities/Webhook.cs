using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// An outbound HTTP subscription. When a matching event occurs, a signed JSON
/// payload is POSTed to <see cref="Url"/> and the attempt recorded as a delivery.
/// </summary>
public class Webhook
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    /// <summary>Shared secret used to compute the HMAC-SHA256 signature header.</summary>
    public string Secret { get; set; } = string.Empty;

    public WebhookEventType EventType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}
