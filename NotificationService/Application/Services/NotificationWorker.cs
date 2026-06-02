using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Application.Services;

/// <summary>
/// Periodically drains the work queue:
///   * delivers due scheduled notifications and retries failed-but-retryable ones
///     (both are persisted as <see cref="NotificationStatus.Pending"/>),
///   * fires due reminders, materializing them into notifications and rescheduling
///     recurring ones.
/// </summary>
public class NotificationWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = configuration.GetValue("Notifications:WorkerIntervalSeconds", 30);
        var interval = TimeSpan.FromSeconds(Math.Max(5, intervalSeconds));

        // Give the database/migrations time to come up before the first sweep.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification worker sweep failed; will retry next interval.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        var webhookPublisher = scope.ServiceProvider.GetRequiredService<IWebhookPublisher>();

        await ProcessDueNotificationsAsync(dbContext, dispatcher, cancellationToken);
        await ProcessDueRemindersAsync(dbContext, dispatcher, webhookPublisher, cancellationToken);
    }

    private static async Task ProcessDueNotificationsAsync(
        NotificationDbContext dbContext,
        INotificationDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var due = await dbContext.Notifications
            .Where(notification => notification.Status == NotificationStatus.Pending
                && notification.AttemptCount < notification.MaxAttempts
                && (notification.ScheduledAt == null || notification.ScheduledAt <= now))
            .OrderBy(notification => notification.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var notification in due)
        {
            await dispatcher.DispatchAsync(notification, cancellationToken);
        }
    }

    private async Task ProcessDueRemindersAsync(
        NotificationDbContext dbContext,
        INotificationDispatcher dispatcher,
        IWebhookPublisher webhookPublisher,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var due = await dbContext.Reminders
            .Where(reminder => reminder.IsActive && reminder.RemindAt <= now)
            .OrderBy(reminder => reminder.RemindAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var reminder in due)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = reminder.UserId,
                Channel = reminder.Channel,
                Recipient = reminder.Recipient,
                Subject = reminder.Title,
                Body = reminder.Message,
                Status = NotificationStatus.Pending,
                MaxAttempts = 3,
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync(cancellationToken);

            await dispatcher.DispatchAsync(notification, cancellationToken);

            await SafePublishReminderAsync(webhookPublisher, reminder, notification, cancellationToken);

            reminder.LastTriggeredAt = now;
            Reschedule(reminder, now);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SafePublishReminderAsync(
        IWebhookPublisher webhookPublisher,
        Reminder reminder,
        Notification notification,
        CancellationToken cancellationToken)
    {
        try
        {
            await webhookPublisher.PublishAsync(
                WebhookEventType.ReminderTriggered,
                new
                {
                    ReminderId = reminder.Id,
                    reminder.UserId,
                    reminder.Title,
                    NotificationId = notification.Id,
                    OccurredAt = DateTime.UtcNow
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to publish ReminderTriggered webhook for reminder {Id}.", reminder.Id);
        }
    }

    private static void Reschedule(Reminder reminder, DateTime now)
    {
        if (reminder.Recurrence == RecurrenceType.None)
        {
            reminder.IsActive = false;
            return;
        }

        var next = reminder.RemindAt;
        while (next <= now)
        {
            next = reminder.Recurrence switch
            {
                RecurrenceType.Daily => next.AddDays(1),
                RecurrenceType.Weekly => next.AddDays(7),
                RecurrenceType.Monthly => next.AddMonths(1),
                _ => next.AddDays(1)
            };
        }

        reminder.RemindAt = next;
    }
}
