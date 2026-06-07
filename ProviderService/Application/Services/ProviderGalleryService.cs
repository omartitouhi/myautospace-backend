using Microsoft.EntityFrameworkCore;
using ProviderService.Application.DTOs;
using ProviderService.Application.Interfaces;
using ProviderService.Domain.Entities;
using ProviderService.Infrastructure.Data;

namespace ProviderService.Application.Services;

public class ProviderGalleryService(ProviderDbContext dbContext) : IProviderGalleryService
{
    private const int MaxImagesPerProfile = 20;

    public async Task<ProviderGalleryResponse> AddImageAsync(Guid profileId, AddGalleryImageRequest request, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        var imageCount = await dbContext.ProviderGalleries
            .CountAsync(g => g.ProviderProfileId == profileId);

        if (imageCount >= MaxImagesPerProfile)
        {
            throw new InvalidOperationException($"A provider profile cannot have more than {MaxImagesPerProfile} gallery images.");
        }

        var image = new ProviderGallery
        {
            Id = Guid.NewGuid(),
            ProviderProfileId = profileId,
            ImageUrl = request.ImageUrl.Trim(),
            Caption = request.Caption?.Trim(),
            DisplayOrder = request.DisplayOrder,
            UploadedAt = DateTime.UtcNow
        };

        dbContext.ProviderGalleries.Add(image);
        await dbContext.SaveChangesAsync();

        return ProviderGalleryResponse.FromEntity(image);
    }

    public async Task DeleteImageAsync(Guid profileId, Guid imageId, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        var image = await dbContext.ProviderGalleries
            .FirstOrDefaultAsync(g => g.Id == imageId && g.ProviderProfileId == profileId)
            ?? throw new KeyNotFoundException($"Gallery image '{imageId}' was not found.");

        dbContext.ProviderGalleries.Remove(image);
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
}
