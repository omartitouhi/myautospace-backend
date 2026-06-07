using MapService.Domain.Entities;
using MapService.Domain.Enums;

namespace MapService.Application.DTOs;

public record MapLocationResponse(
    Guid Id,
    Guid EntityId,
    EntityType EntityType,
    double Latitude,
    double Longitude,
    string? Address,
    string City,
    string Country,
    Guid OwnerAuthUserId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static MapLocationResponse FromEntity(MapLocation location) => new(
        location.Id,
        location.EntityId,
        location.EntityType,
        location.Latitude,
        location.Longitude,
        location.Address,
        location.City,
        location.Country,
        location.OwnerAuthUserId,
        location.IsActive,
        location.CreatedAt,
        location.UpdatedAt);
}
