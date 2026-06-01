namespace UserService.Domain.Entities;

public class UserPack
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string PackType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
