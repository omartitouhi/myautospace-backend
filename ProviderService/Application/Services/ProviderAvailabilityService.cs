using Microsoft.EntityFrameworkCore;
using ProviderService.Application.DTOs;
using ProviderService.Application.Interfaces;
using ProviderService.Domain.Entities;
using ProviderService.Infrastructure.Data;

namespace ProviderService.Application.Services;

public class ProviderAvailabilityService(ProviderDbContext dbContext) : IProviderAvailabilityService
{
    public async Task<ProviderAvailabilityResponse> SetAsync(Guid profileId, SetAvailabilityRequest request, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        ValidateTimeRange(request.StartTime, request.EndTime);

        // Upsert: replace existing slot for same DayOfWeek
        var existing = await dbContext.ProviderAvailabilities
            .FirstOrDefaultAsync(a => a.ProviderProfileId == profileId && a.DayOfWeek == request.DayOfWeek);

        if (existing is not null)
        {
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
            existing.IsAvailable = request.IsAvailable;
            await dbContext.SaveChangesAsync();
            return ProviderAvailabilityResponse.FromEntity(existing);
        }

        var availability = new ProviderAvailability
        {
            Id = Guid.NewGuid(),
            ProviderProfileId = profileId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = request.IsAvailable
        };

        dbContext.ProviderAvailabilities.Add(availability);
        await dbContext.SaveChangesAsync();

        return ProviderAvailabilityResponse.FromEntity(availability);
    }

    public async Task<IReadOnlyCollection<ProviderAvailabilityResponse>> GetByProfileAsync(Guid profileId)
    {
        var availabilities = await dbContext.ProviderAvailabilities
            .AsNoTracking()
            .Where(a => a.ProviderProfileId == profileId)
            .OrderBy(a => a.DayOfWeek)
            .ToListAsync();

        return availabilities.Select(ProviderAvailabilityResponse.FromEntity).ToList();
    }

    public async Task DeleteAsync(Guid profileId, Guid availabilityId, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        var availability = await dbContext.ProviderAvailabilities
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.ProviderProfileId == profileId)
            ?? throw new KeyNotFoundException($"Availability slot '{availabilityId}' was not found.");

        dbContext.ProviderAvailabilities.Remove(availability);
        await dbContext.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<ProviderProfile> GetProfileAndEnsureOwnershipAsync(Guid profileId, Guid authUserId)
    {
        var profile = await dbContext.ProviderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profileId && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider profile '{profileId}' was not found.");

        if (profile.AuthUserId != authUserId)
        {
            throw new UnauthorizedAccessException("You do not own this provider profile.");
        }

        return profile;
    }

    private static void ValidateTimeRange(TimeOnly start, TimeOnly end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End time must be after start time.");
        }
    }
}
