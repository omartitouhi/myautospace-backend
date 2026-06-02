using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services;

public class UserActivityService(UserDbContext dbContext) : IUserActivityService
{
    public const string Login = "Login";

    public const string ProfileUpdated = "ProfileUpdated";

    public const string ProfileCreated = "ProfileCreated";

    public const string DocumentUploaded = "DocumentUploaded";

    public const string PackChanged = "PackChanged";

    public const string IdentityVerificationChanged = "IdentityVerificationChanged";

    public void Log(UserProfile userProfile, string action, string description)
    {
        dbContext.UserActivities.Add(new UserActivity
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<IReadOnlyCollection<UserActivity>> GetActivitiesAsync(Guid authUserId)
    {
        return await dbContext.UserActivities
            .AsNoTracking()
            .Where(activity => activity.UserProfile.AuthUserId == authUserId)
            .OrderByDescending(activity => activity.CreatedAt)
            .ToListAsync();
    }
}
