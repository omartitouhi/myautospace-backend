namespace UserService.Application.DTOs;

public record UpdatePreferencesRequest(
    string Language,
    string Currency,
    bool NotificationEmail,
    bool NotificationSms,
    bool NotificationPush);
