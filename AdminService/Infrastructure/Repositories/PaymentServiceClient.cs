using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;

namespace AdminService.Infrastructure.Repositories;

public class PaymentServiceClient : IPaymentServiceClient
{
    private const string NotImplementedMessage = "PaymentService HTTP integration is not implemented yet.";

    public Task<IReadOnlyList<AdminPaymentResponse>> GetPaymentsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task<AdminPaymentResponse?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task<AdminPaymentStatsResponse> GetPaymentStatsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task RequestRefundAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }
}
