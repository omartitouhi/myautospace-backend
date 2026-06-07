using MapService.Domain.Enums;

namespace MapService.Domain.Entities;

public class MapLocation
{
    public Guid Id { get; set; }

    /// <summary>ID of the entity in its owning service (Vehicle.Id or ProviderProfile.Id).</summary>
    public Guid EntityId { get; set; }

    public EntityType EntityType { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? Address { get; set; }

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    /// <summary>AuthUserId of the owner in the source service.</summary>
    public Guid OwnerAuthUserId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
