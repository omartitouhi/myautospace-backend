using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record UserProfileResponse(
    Guid Id,
    Guid AuthUserId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string PhoneNumber,
    string Address,
    string Country,
    string City,
    string? ProfilePictureUrl,
    string? Bio,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    UserPackResponse? UserPack,
    IReadOnlyCollection<IdentityVerificationResponse> IdentityVerifications,
    IReadOnlyCollection<UserDocumentResponse> UserDocuments,
    UserPreferenceResponse? UserPreference,
    IReadOnlyCollection<UserActivityResponse> UserActivities,
    TrustScoreResponse? TrustScore);

public record UserPackResponse(
    Guid Id,
    PackType PackType,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive);

public record IdentityVerificationResponse(
    Guid Id,
    VerificationStatus VerificationStatus,
    DateTime? VerifiedAt,
    string? RejectionReason);

public record UserDocumentResponse(
    Guid Id,
    DocumentType DocumentType,
    string FileUrl,
    DateTime UploadedAt,
    VerificationStatus Status);

public record UserPreferenceResponse(
    Guid Id,
    string Language,
    string Currency,
    bool NotificationEmail,
    bool NotificationSms,
    bool NotificationPush);

public record UserActivityResponse(
    Guid Id,
    string Action,
    string Description,
    DateTime CreatedAt);

public record TrustScoreResponse(
    Guid Id,
    int Score,
    DateTime LastCalculatedAt);
