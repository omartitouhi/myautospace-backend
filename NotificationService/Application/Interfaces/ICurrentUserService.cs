using NotificationService.Application.Security;

namespace NotificationService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
