using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IPaymentServiceClient
{
    Task<IReadOnlyList<AdminPaymentResponse>> GetPaymentsAsync(CancellationToken cancellationToken = default);

    Task<AdminPaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AdminPaymentStatsResponse> GetPaymentStatsAsync(CancellationToken cancellationToken = default);

    Task RequestRefundAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);
}
