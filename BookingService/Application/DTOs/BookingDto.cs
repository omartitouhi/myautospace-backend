namespace BookingService.Application.DTOs;

public record BookingDto(
    Guid Id,
    string ExternalCustomerId,
    Guid ProviderId,
    DateTime StartUtc,
    DateTime EndUtc,
    string Status
);

