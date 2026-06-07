using ProviderService.Domain.Entities;

namespace ProviderService.Application.DTOs;

public record ProviderAvailabilityResponse(
    Guid Id,
    Guid ProviderProfileId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable)
{
    public static ProviderAvailabilityResponse FromEntity(ProviderAvailability a) => new(
        a.Id,
        a.ProviderProfileId,
        a.DayOfWeek,
        a.StartTime,
        a.EndTime,
        a.IsAvailable);
}
