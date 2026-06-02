using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record CreateWebhookRequest(
    string Name,
    string Url,
    string Secret,
    WebhookEventType EventType);

public record UpdateWebhookRequest(
    string? Name,
    string? Url,
    string? Secret,
    WebhookEventType? EventType,
    bool? IsActive);

public record WebhookResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Url,
    WebhookEventType EventType,
    bool IsActive,
    DateTime CreatedAt);

public record WebhookDeliveryResponse(
    Guid Id,
    WebhookEventType EventType,
    bool Success,
    int? ResponseStatusCode,
    int AttemptCount,
    string? LastError,
    DateTime CreatedAt);
