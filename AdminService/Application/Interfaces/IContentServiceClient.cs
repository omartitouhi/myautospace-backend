using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IContentServiceClient
{
    Task<IReadOnlyList<AdminContentResponse>> GetContentAsync(CancellationToken cancellationToken = default);

    Task<AdminContentResponse?> GetContentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task ApproveContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);

    Task RejectContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);

    Task RemoveContentAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);
}
