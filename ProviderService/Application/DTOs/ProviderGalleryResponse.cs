using ProviderService.Domain.Entities;

namespace ProviderService.Application.DTOs;

public record ProviderGalleryResponse(
    Guid Id,
    Guid ProviderProfileId,
    string ImageUrl,
    string? Caption,
    int DisplayOrder,
    DateTime UploadedAt)
{
    public static ProviderGalleryResponse FromEntity(ProviderGallery g) => new(
        g.Id,
        g.ProviderProfileId,
        g.ImageUrl,
        g.Caption,
        g.DisplayOrder,
        g.UploadedAt);
}
