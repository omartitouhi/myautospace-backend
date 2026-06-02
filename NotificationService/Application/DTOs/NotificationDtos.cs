using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record SendNotificationRequest(
    Guid RecipientUserId,
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string Body,
    DateTime? ScheduledAt,
    int? MaxAttempts);

public record NotificationResponse(
    Guid Id,
    Guid RecipientUserId,
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string Body,
    NotificationStatus Status,
    int AttemptCount,
    int MaxAttempts,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    string? LastError,
    DateTime CreatedAt);

public record NotificationAttemptResponse(
    Guid Id,
    int AttemptNumber,
    AttemptStatus Status,
    string? Detail,
    DateTime AttemptedAt);
