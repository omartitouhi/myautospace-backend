using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }

    public Guid AuthUserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public string? Bio { get; set; }

    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<UserPack> UserPacks { get; set; } = new List<UserPack>();

    public ICollection<IdentityVerification> IdentityVerifications { get; set; } = new List<IdentityVerification>();

    public ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();

    public UserPreference? UserPreference { get; set; }

    public ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();

    public TrustScore? TrustScore { get; set; }
}
