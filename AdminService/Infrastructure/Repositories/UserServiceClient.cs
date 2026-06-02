using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;

namespace AdminService.Infrastructure.Repositories;

public class UserServiceClient : IUserServiceClient
{
    private const string NotImplementedMessage = "UserService HTTP integration is not implemented yet.";

    public Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task<AdminUserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task SuspendUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task ActivateUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }

    public Task BlockUserAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotImplementedMessage);
    }
}
