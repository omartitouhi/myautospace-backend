using VehicleService.Domain.Enums;

namespace VehicleService.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }

    public Guid OwnerAuthUserId { get; set; }

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string? VIN { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public int Mileage { get; set; }

    public FuelType FuelType { get; set; }

    public TransmissionType Transmission { get; set; }

    public BodyType BodyType { get; set; }

    public ListingType ListingType { get; set; }

    public VehicleStatus Status { get; set; }

    public string? Color { get; set; }

    public string Country { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
