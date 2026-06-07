using Microsoft.EntityFrameworkCore;
using ProviderService.Application.DTOs;
using ProviderService.Application.Interfaces;
using ProviderService.Domain.Entities;
using ProviderService.Domain.Enums;
using ProviderService.Infrastructure.Data;

namespace ProviderService.Application.Services;

public class ProviderProfileService(ProviderDbContext dbContext) : IProviderProfileService
{
    public async Task<ProviderProfileResponse> CreateAsync(CreateProviderProfileRequest request, Guid authUserId)
    {
        ValidateStringNotEmpty(request.BusinessName, nameof(request.BusinessName));
        ValidateStringNotEmpty(request.City, nameof(request.City));
        ValidateStringNotEmpty(request.Country, nameof(request.Country));

        var existing = await dbContext.ProviderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AuthUserId == authUserId && !p.IsDeleted);

        if (existing is not null)
        {
            throw new InvalidOperationException("A provider profile already exists for this account.");
        }

        var profile = new ProviderProfile
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            BusinessName = request.BusinessName.Trim(),
            Description = request.Description?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            Status = ProviderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.ProviderProfiles.Add(profile);
        await dbContext.SaveChangesAsync();

        return ProviderProfileResponse.FromEntity(await LoadFullProfileAsync(profile.Id));
    }

    public async Task<ProviderProfileResponse> GetByIdAsync(Guid id)
    {
        var profile = await LoadFullProfileAsync(id);
        return ProviderProfileResponse.FromEntity(profile);
    }

    public async Task<ProviderProfileResponse> GetByAuthUserIdAsync(Guid authUserId)
    {
        var profile = await dbContext.ProviderProfiles
            .AsNoTracking()
            .Include(p => p.Services)
            .Include(p => p.Availabilities)
            .Include(p => p.GalleryImages)
            .FirstOrDefaultAsync(p => p.AuthUserId == authUserId && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"No provider profile found for user '{authUserId}'.");

        return ProviderProfileResponse.FromEntity(profile);
    }

    public async Task<IReadOnlyCollection<ProviderProfileSummaryResponse>> GetAllActiveAsync()
    {
        var profiles = await dbContext.ProviderProfiles
            .AsNoTracking()
            .Where(p => p.Status == ProviderStatus.Active && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return profiles.Select(ProviderProfileSummaryResponse.FromEntity).ToList();
    }

    public async Task<ProviderProfileResponse> UpdateAsync(Guid id, UpdateProviderProfileRequest request, Guid authUserId)
    {
        var profile = await GetProfileAndEnsureOwnershipAsync(id, authUserId);

        if (request.BusinessName is not null) ValidateStringNotEmpty(request.BusinessName, nameof(request.BusinessName));
        if (request.City is not null) ValidateStringNotEmpty(request.City, nameof(request.City));
        if (request.Country is not null) ValidateStringNotEmpty(request.Country, nameof(request.Country));

        if (request.BusinessName is not null) profile.BusinessName = request.BusinessName.Trim();
        if (request.Description is not null) profile.Description = request.Description.Trim();
        if (request.PhoneNumber is not null) profile.PhoneNumber = request.PhoneNumber.Trim();
        if (request.Address is not null) profile.Address = request.Address.Trim();
        if (request.City is not null) profile.City = request.City.Trim();
        if (request.Country is not null) profile.Country = request.Country.Trim();
        profile.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return ProviderProfileResponse.FromEntity(await LoadFullProfileAsync(profile.Id));
    }

    public async Task DeleteAsync(Guid id, Guid authUserId)
    {
        var profile = await GetProfileAndEnsureOwnershipAsync(id, authUserId);

        profile.IsDeleted = true;
        profile.DeletedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<ProviderProfile> LoadFullProfileAsync(Guid id)
    {
        return await dbContext.ProviderProfiles
            .Include(p => p.Services)
            .Include(p => p.Availabilities)
            .Include(p => p.GalleryImages)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider profile '{id}' was not found.");
    }

    private async Task<ProviderProfile> GetProfileAndEnsureOwnershipAsync(Guid id, Guid authUserId)
    {
        var profile = await dbContext.ProviderProfiles
            .Include(p => p.Services)
            .Include(p => p.Availabilities)
            .Include(p => p.GalleryImages)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider profile '{id}' was not found.");

        if (profile.AuthUserId != authUserId)
        {
            throw new UnauthorizedAccessException("You do not own this provider profile.");
        }

        return profile;
    }

    private static void ValidateStringNotEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{fieldName}' cannot be empty or whitespace.", fieldName);
        }
    }
}
