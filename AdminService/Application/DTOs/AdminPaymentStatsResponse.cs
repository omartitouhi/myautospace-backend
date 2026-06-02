namespace AdminService.Application.DTOs;

public record AdminPaymentStatsResponse(
    int TotalPayments,
    decimal TotalAmount,
    int PendingPayments,
    int FailedPayments,
    int RefundRequests);
