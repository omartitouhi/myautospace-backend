namespace BookingService.Application.DTOs;

public record BookingDto(
    Guid Id,
    Guid CustomerUserId,
    Guid ProviderUserId,
    Guid? VehicleId,
    string ServiceType,
    DateTime ScheduledAt,
    int DurationMinutes,
    string Status
);

