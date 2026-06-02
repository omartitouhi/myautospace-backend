using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<NotificationAttempt> NotificationAttempts { get; set; } = null!;

    public DbSet<Reminder> Reminders { get; set; } = null!;

    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    public DbSet<Webhook> Webhooks { get; set; } = null!;

    public DbSet<WebhookDelivery> WebhookDeliveries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>()
            .HasIndex(notification => notification.RecipientUserId);

        modelBuilder.Entity<Notification>()
            .HasIndex(notification => notification.Status);

        modelBuilder.Entity<Notification>()
            .HasMany(notification => notification.Attempts)
            .WithOne(attempt => attempt.Notification)
            .HasForeignKey(attempt => attempt.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reminder>()
            .HasIndex(reminder => reminder.UserId);

        modelBuilder.Entity<NotificationPreference>()
            .HasIndex(preference => preference.UserId)
            .IsUnique();

        modelBuilder.Entity<Webhook>()
            .HasIndex(webhook => webhook.UserId);

        modelBuilder.Entity<Webhook>()
            .HasMany(webhook => webhook.Deliveries)
            .WithOne(delivery => delivery.Webhook)
            .HasForeignKey(delivery => delivery.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
