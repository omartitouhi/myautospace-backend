using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services;

public class TrustScoreService(UserDbContext dbContext) : ITrustScoreService
{
    private const int MaxScore = 100;

    private const int RegularActivityThreshold = 5;

    public async Task<TrustScore?> GetAsync(Guid authUserId)
    {
        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.TrustScore)
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        return userProfile?.TrustScore;
    }

    public async Task<TrustScore?> RecalculateAsync(Guid authUserId)
    {
        var userProfile = await dbContext.UserProfiles
            .Include(profile => profile.IdentityVerifications)
            .Include(profile => profile.UserDocuments)
            .Include(profile => profile.UserPacks)
            .Include(profile => profile.UserActivities)
            .Include(profile => profile.TrustScore)
            .FirstOrDefaultAsync(profile => profile.AuthUserId == authUserId);

        if (userProfile is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var score = Calculate(userProfile, now);

        if (userProfile.TrustScore is null)
        {
            userProfile.TrustScore = new TrustScore
            {
                Id = Guid.NewGuid(),
                UserProfileId = userProfile.Id
            };

            dbContext.TrustScores.Add(userProfile.TrustScore);
        }

        userProfile.TrustScore.Score = score;
        userProfile.TrustScore.LastCalculatedAt = now;

        await dbContext.SaveChangesAsync();

        return userProfile.TrustScore;
    }

    private static int Calculate(UserProfile userProfile, DateTime now)
    {
        var score = 0;

        if (userProfile.IdentityVerifications.Any(verification =>
            verification.VerificationStatus == VerificationStatus.Approved))
        {
            score += 20;
        }

        if (userProfile.UserDocuments.Any(document =>
            document.Status == VerificationStatus.Approved))
        {
            score += 20;
        }

        if (userProfile.CreatedAt <= now.AddMonths(-6))
        {
            score += 20;
        }

        if (userProfile.UserPacks.Any(pack =>
            pack.IsActive
            && pack.StartDate <= now
            && (pack.EndDate is null || pack.EndDate >= now)
            && (pack.PackType == PackType.Premium || pack.PackType == PackType.Pro)))
        {
            score += 20;
        }

        if (userProfile.UserActivities.Count(activity => activity.CreatedAt >= now.AddDays(-30)) >= RegularActivityThreshold)
        {
            score += 20;
        }

        return Math.Min(score, MaxScore);
    }
}
