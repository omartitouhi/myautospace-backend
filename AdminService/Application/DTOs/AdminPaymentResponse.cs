namespace AdminService.Application.DTOs;

public record AdminPaymentResponse(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt);
