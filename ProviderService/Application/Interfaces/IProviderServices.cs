using ProviderService.Application.DTOs;

namespace ProviderService.Application.Interfaces;

public interface IProviderProfileService
{
    Task<ProviderProfileResponse> CreateAsync(CreateProviderProfileRequest request, Guid authUserId);
    Task<ProviderProfileResponse> GetByIdAsync(Guid id);
    Task<ProviderProfileResponse> GetByAuthUserIdAsync(Guid authUserId);
    Task<IReadOnlyCollection<ProviderProfileSummaryResponse>> GetAllActiveAsync();
    Task<ProviderProfileResponse> UpdateAsync(Guid id, UpdateProviderProfileRequest request, Guid authUserId);
    Task DeleteAsync(Guid id, Guid authUserId);
}

public interface IServiceOfferingService
{
    Task<ServiceOfferingResponse> AddAsync(Guid profileId, CreateServiceOfferingRequest request, Guid authUserId);
    Task<ServiceOfferingResponse> UpdateAsync(Guid profileId, Guid serviceId, UpdateServiceOfferingRequest request, Guid authUserId);
    Task DeleteAsync(Guid profileId, Guid serviceId, Guid authUserId);
}

public interface IProviderAvailabilityService
{
    Task<ProviderAvailabilityResponse> SetAsync(Guid profileId, SetAvailabilityRequest request, Guid authUserId);
    Task<IReadOnlyCollection<ProviderAvailabilityResponse>> GetByProfileAsync(Guid profileId);
    Task DeleteAsync(Guid profileId, Guid availabilityId, Guid authUserId);
}

public interface IProviderGalleryService
{
    Task<ProviderGalleryResponse> AddImageAsync(Guid profileId, AddGalleryImageRequest request, Guid authUserId);
    Task DeleteImageAsync(Guid profileId, Guid imageId, Guid authUserId);
}
