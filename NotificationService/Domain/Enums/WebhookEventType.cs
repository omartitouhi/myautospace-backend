namespace NotificationService.Domain.Enums;

public enum WebhookEventType
{
    NotificationSent,
    NotificationFailed,
    ReminderTriggered,
    Custom
}
