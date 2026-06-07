using ProviderService.Application.Security;

namespace ProviderService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
