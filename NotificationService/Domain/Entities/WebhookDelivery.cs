using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>Audit record for a single webhook POST attempt.</summary>
public class WebhookDelivery
{
    public Guid Id { get; set; }

    public Guid WebhookId { get; set; }

    public Webhook Webhook { get; set; } = null!;

    public WebhookEventType EventType { get; set; }

    public string Payload { get; set; } = string.Empty;

    public bool Success { get; set; }

    public int? ResponseStatusCode { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }
}
