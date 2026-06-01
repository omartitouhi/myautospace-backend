using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserActivityService
{
    void Log(UserProfile userProfile, string action, string description);

    Task<IReadOnlyCollection<UserActivity>> GetActivitiesAsync(Guid authUserId);
}
