namespace UserService.Domain.Entities;

public class UserPreference
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string Language { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public bool NotificationEmail { get; set; }

    public bool NotificationSms { get; set; }

    public bool NotificationPush { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
