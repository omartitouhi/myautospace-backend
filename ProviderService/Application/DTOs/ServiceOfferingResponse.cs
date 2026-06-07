using ProviderService.Domain.Entities;
using ProviderService.Domain.Enums;

namespace ProviderService.Application.DTOs;

public record ServiceOfferingResponse(
    Guid Id,
    Guid ProviderProfileId,
    string Name,
    string? Description,
    decimal Price,
    int DurationMinutes,
    ServiceCategory Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ServiceOfferingResponse FromEntity(ServiceOffering s) => new(
        s.Id,
        s.ProviderProfileId,
        s.Name,
        s.Description,
        s.Price,
        s.DurationMinutes,
        s.Category,
        s.IsActive,
        s.CreatedAt,
        s.UpdatedAt);
}
