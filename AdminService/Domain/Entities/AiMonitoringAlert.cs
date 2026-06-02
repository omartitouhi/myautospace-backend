using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

public class AiMonitoringAlert
{
    public Guid Id { get; set; }

    public string SourceService { get; set; } = string.Empty;

    public string AlertType { get; set; } = string.Empty;

    public AlertSeverity Severity { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? DataJson { get; set; }

    public AlertStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
}
