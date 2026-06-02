using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record CreateReminderRequest(
    string Title,
    string Message,
    NotificationChannel Channel,
    string Recipient,
    DateTime RemindAt,
    RecurrenceType? Recurrence);

public record UpdateReminderRequest(
    string? Title,
    string? Message,
    NotificationChannel? Channel,
    string? Recipient,
    DateTime? RemindAt,
    RecurrenceType? Recurrence,
    bool? IsActive);

public record ReminderResponse(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    NotificationChannel Channel,
    string Recipient,
    DateTime RemindAt,
    RecurrenceType Recurrence,
    bool IsActive,
    DateTime? LastTriggeredAt,
    DateTime CreatedAt);
