using UserService.Application.Security;

namespace UserService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
