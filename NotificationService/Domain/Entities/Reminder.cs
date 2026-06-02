using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// A scheduled (optionally recurring) reminder. When due, the background worker
/// materializes it into a <see cref="Notification"/> and dispatches it.
/// </summary>
public class Reminder
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationChannel Channel { get; set; }

    /// <summary>Destination for the generated notification (email/phone/device token).</summary>
    public string Recipient { get; set; } = string.Empty;

    public DateTime RemindAt { get; set; }

    public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;

    public bool IsActive { get; set; } = true;

    public DateTime? LastTriggeredAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
