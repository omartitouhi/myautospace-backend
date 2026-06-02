using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// A single message to be delivered to a user over one channel (email, SMS or push).
/// Carries its own retry budget and an audit trail of delivery attempts.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }

    public Guid RecipientUserId { get; set; }

    public NotificationChannel Channel { get; set; }

    /// <summary>Email address, phone number or device token, depending on the channel.</summary>
    public string Recipient { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; } = 3;

    /// <summary>When set in the future, the notification is delivered by the background worker.</summary>
    public DateTime? ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<NotificationAttempt> Attempts { get; set; } = new List<NotificationAttempt>();
}
