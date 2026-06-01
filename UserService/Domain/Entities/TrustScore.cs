namespace UserService.Domain.Entities;

public class TrustScore
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public int Score { get; set; }

    public DateTime LastCalculatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
