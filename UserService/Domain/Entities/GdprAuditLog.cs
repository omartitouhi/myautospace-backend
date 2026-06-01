namespace UserService.Domain.Entities;

public class GdprAuditLog
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
