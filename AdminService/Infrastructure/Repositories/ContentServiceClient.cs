using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;

namespace AdminService.Infrastructure.Repositories;

public class ContentServiceClient : IContentServiceClient
{
    private const string NotImplementedMessage = "Content HTTP integration with VehicleService and MediaService is not implemented yet.";

    public Task<IReadOnlyList<AdminContentResponse>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task<AdminContentResponse?> GetContentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task ApproveContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task RejectContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task RemoveContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }
}
