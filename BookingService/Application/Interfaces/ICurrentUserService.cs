using BookingService.Application.Security;

namespace BookingService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
