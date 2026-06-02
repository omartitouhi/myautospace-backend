namespace NotificationService.Application.DTOs;

public record UpdatePreferencesRequest(
    bool? EmailEnabled,
    bool? SmsEnabled,
    bool? PushEnabled,
    bool? MarketingOptIn);

public record PreferenceResponse(
    Guid Id,
    Guid UserId,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool MarketingOptIn,
    DateTime UpdatedAt);
