using Microsoft.EntityFrameworkCore;
using ProviderService.Application.DTOs;
using ProviderService.Application.Interfaces;
using ProviderService.Domain.Entities;
using ProviderService.Infrastructure.Data;

namespace ProviderService.Application.Services;

public class ServiceOfferingService(ProviderDbContext dbContext) : IServiceOfferingService
{
    public async Task<ServiceOfferingResponse> AddAsync(Guid profileId, CreateServiceOfferingRequest request, Guid authUserId)
    {
        var profile = await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        ValidateStringNotEmpty(request.Name, nameof(request.Name));
        ValidatePrice(request.Price);
        ValidateDuration(request.DurationMinutes);

        var offering = new ServiceOffering
        {
            Id = Guid.NewGuid(),
            ProviderProfileId = profile.Id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            Category = request.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.ServiceOfferings.Add(offering);
        await dbContext.SaveChangesAsync();

        return ServiceOfferingResponse.FromEntity(offering);
    }

    public async Task<ServiceOfferingResponse> UpdateAsync(Guid profileId, Guid serviceId, UpdateServiceOfferingRequest request, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        var offering = await dbContext.ServiceOfferings
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.ProviderProfileId == profileId)
            ?? throw new KeyNotFoundException($"Service offering '{serviceId}' was not found.");

        if (request.Name is not null) ValidateStringNotEmpty(request.Name, nameof(request.Name));
        if (request.Price.HasValue) ValidatePrice(request.Price.Value);
        if (request.DurationMinutes.HasValue) ValidateDuration(request.DurationMinutes.Value);

        if (request.Name is not null) offering.Name = request.Name.Trim();
        if (request.Description is not null) offering.Description = request.Description.Trim();
        if (request.Price.HasValue) offering.Price = request.Price.Value;
        if (request.DurationMinutes.HasValue) offering.DurationMinutes = request.DurationMinutes.Value;
        if (request.Category.HasValue) offering.Category = request.Category.Value;
        if (request.IsActive.HasValue) offering.IsActive = request.IsActive.Value;
        offering.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return ServiceOfferingResponse.FromEntity(offering);
    }

    public async Task DeleteAsync(Guid profileId, Guid serviceId, Guid authUserId)
    {
        await GetProfileAndEnsureOwnershipAsync(profileId, authUserId);

        var offering = await dbContext.ServiceOfferings
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.ProviderProfileId == profileId)
            ?? throw new KeyNotFoundException($"Service offering '{serviceId}' was not found.");

        dbContext.ServiceOfferings.Remove(offering);
        await dbContext.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Domain.Entities.ProviderProfile> GetProfileAndEnsureOwnershipAsync(Guid profileId, Guid authUserId)
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

    private static void ValidateStringNotEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{fieldName}' cannot be empty or whitespace.", fieldName);
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
        }
    }

    private static void ValidateDuration(int minutes)
    {
        if (minutes is < 1 or > 1440)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), "Duration must be between 1 and 1440 minutes.");
        }
    }
}
