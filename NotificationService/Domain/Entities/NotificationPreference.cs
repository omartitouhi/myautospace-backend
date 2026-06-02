namespace NotificationService.Domain.Entities;

/// <summary>Per-user channel opt-in/opt-out. A disabled channel suppresses delivery.</summary>
public class NotificationPreference
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public bool EmailEnabled { get; set; } = true;

    public bool SmsEnabled { get; set; } = true;

    public bool PushEnabled { get; set; } = true;

    public bool MarketingOptIn { get; set; }

    public DateTime UpdatedAt { get; set; }
}
