using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IUserServiceClient
{
    Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<AdminUserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SuspendUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);

    Task ActivateUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);

    Task BlockUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default);
}
