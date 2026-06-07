using MapService.Application.Security;

namespace MapService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
