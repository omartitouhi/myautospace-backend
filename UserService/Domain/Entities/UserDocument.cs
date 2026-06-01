using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class UserDocument
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public DocumentType DocumentType { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public VerificationStatus Status { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
