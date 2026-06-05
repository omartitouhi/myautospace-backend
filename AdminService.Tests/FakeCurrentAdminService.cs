using AdminService.Application.Interfaces;

namespace AdminService.Tests;

internal sealed class FakeCurrentAdminService(Guid? adminUserId) : ICurrentAdminService
{
    public Guid? GetAdminUserId() => adminUserId;
}
