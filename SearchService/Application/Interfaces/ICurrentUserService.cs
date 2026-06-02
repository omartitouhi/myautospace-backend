using SearchService.Application.Security;

namespace SearchService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
