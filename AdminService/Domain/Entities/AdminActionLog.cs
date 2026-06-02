namespace AdminService.Domain.Entities;

public class AdminActionLog
{
    public Guid Id { get; set; }

    public Guid AdminUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string TargetService { get; set; } = string.Empty;

    public string TargetEntity { get; set; } = string.Empty;

    public Guid? TargetEntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
