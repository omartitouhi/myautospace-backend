using ProviderService.Domain.Entities;
using ProviderService.Domain.Enums;

namespace ProviderService.Application.DTOs;

public record ProviderProfileResponse(
    Guid Id,
    Guid AuthUserId,
    string BusinessName,
    string? Description,
    string? PhoneNumber,
    string? Address,
    string City,
    string Country,
    ProviderStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ServiceOfferingResponse> Services,
    IReadOnlyCollection<ProviderAvailabilityResponse> Availabilities,
    IReadOnlyCollection<ProviderGalleryResponse> GalleryImages)
{
    public static ProviderProfileResponse FromEntity(ProviderProfile profile) => new(
        profile.Id,
        profile.AuthUserId,
        profile.BusinessName,
        profile.Description,
        profile.PhoneNumber,
        profile.Address,
        profile.City,
        profile.Country,
        profile.Status,
        profile.CreatedAt,
        profile.UpdatedAt,
        profile.Services.Select(ServiceOfferingResponse.FromEntity).ToList(),
        profile.Availabilities.Select(ProviderAvailabilityResponse.FromEntity).ToList(),
        profile.GalleryImages.Select(ProviderGalleryResponse.FromEntity).ToList());
}

public record ProviderProfileSummaryResponse(
    Guid Id,
    Guid AuthUserId,
    string BusinessName,
    string? Description,
    string City,
    string Country,
    ProviderStatus Status,
    DateTime CreatedAt)
{
    public static ProviderProfileSummaryResponse FromEntity(ProviderProfile profile) => new(
        profile.Id,
        profile.AuthUserId,
        profile.BusinessName,
        profile.Description,
        profile.City,
        profile.Country,
        profile.Status,
        profile.CreatedAt);
}
