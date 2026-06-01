namespace UserService.Domain.Entities;

public class UserDocument
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public UserProfile UserProfile { get; set; } = null!;
}
