using MapService.Domain.Entities;
using MapService.Domain.Enums;

namespace MapService.Application.DTOs;

public record NearbyLocationResult(
    Guid Id,
    Guid EntityId,
    EntityType EntityType,
    double Latitude,
    double Longitude,
    string? Address,
    string City,
    string Country,
    double DistanceKm)
{
    public static NearbyLocationResult FromEntity(MapLocation location, double distanceKm) => new(
        location.Id,
        location.EntityId,
        location.EntityType,
        location.Latitude,
        location.Longitude,
        location.Address,
        location.City,
        location.Country,
        Math.Round(distanceKm, 3));
}
