using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data;

public class AdminDbContext(DbContextOptions<AdminDbContext> options) : DbContext(options)
{
    public DbSet<AdminActionLog> AdminActionLogs { get; set; } = null!;

    public DbSet<ModerationCase> ModerationCases { get; set; } = null!;

    public DbSet<AdminReport> AdminReports { get; set; } = null!;

    public DbSet<SystemConfig> SystemConfigs { get; set; } = null!;

    public DbSet<AiMonitoringAlert> AiMonitoringAlerts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdminActionLog>()
            .HasIndex(adminActionLog => adminActionLog.AdminUserId);

        modelBuilder.Entity<AdminActionLog>()
            .HasIndex(adminActionLog => adminActionLog.TargetService);

        modelBuilder.Entity<ModerationCase>()
            .HasIndex(moderationCase => moderationCase.Status);

        modelBuilder.Entity<SystemConfig>()
            .HasIndex(systemConfig => systemConfig.Key)
            .IsUnique();

        modelBuilder.Entity<AiMonitoringAlert>()
            .HasIndex(aiMonitoringAlert => aiMonitoringAlert.Severity);

        modelBuilder.Entity<AiMonitoringAlert>()
            .HasIndex(aiMonitoringAlert => aiMonitoringAlert.Status);
    }
}
