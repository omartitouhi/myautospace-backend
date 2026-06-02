using VehicleService.Application.Security;

namespace VehicleService.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
