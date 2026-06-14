namespace BookingService.Application.Interfaces;

/// <summary>
/// Minimal vehicle facts BookingService needs from VehicleService to validate
/// and denormalize a booking (database-per-service: no cross-service EF).
/// </summary>
public record VehicleInfo(
    Guid Id,
    Guid OwnerAuthUserId,
    string Make,
    string Model,
    int Year,
    string City,
    string Country,
    string Status,
    string ListingType,
    decimal Price);

public interface IVehicleLookupClient
{
    /// <summary>Fetches a vehicle by id, or null if it does not exist.</summary>
    Task<VehicleInfo?> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
