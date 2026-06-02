using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>One delivery attempt for a notification — the basis of the history log and retries.</summary>
public class NotificationAttempt
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public Notification Notification { get; set; } = null!;

    public int AttemptNumber { get; set; }

    public AttemptStatus Status { get; set; }

    public string? Detail { get; set; }

    public DateTime AttemptedAt { get; set; }
}
