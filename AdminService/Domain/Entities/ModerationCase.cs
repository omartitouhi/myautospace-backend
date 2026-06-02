using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

public class ModerationCase
{
    public Guid Id { get; set; }

    public string ReportedEntityType { get; set; } = string.Empty;

    public Guid ReportedEntityId { get; set; }

    public Guid? ReportedByUserId { get; set; }

    public Guid? AssignedAdminId { get; set; }

    public ModerationStatus Status { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Decision { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
}
