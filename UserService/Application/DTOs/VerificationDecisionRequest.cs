namespace UserService.Application.DTOs;

public record VerificationDecisionRequest(
    Guid UserProfileId,
    string? RejectionReason);
