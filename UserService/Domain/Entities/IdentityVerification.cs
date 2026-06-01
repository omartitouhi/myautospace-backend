using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class IdentityVerification
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public VerificationStatus VerificationStatus { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? RejectionReason { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
