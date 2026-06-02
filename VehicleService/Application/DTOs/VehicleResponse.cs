using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record VehicleResponse(
    Guid Id,
    Guid OwnerAuthUserId,
    string Make,
    string Model,
    int Year,
    string? VIN,
    decimal Price,
    string? Description,
    int Mileage,
    FuelType FuelType,
    TransmissionType Transmission,
    BodyType BodyType,
    ListingType ListingType,
    VehicleStatus Status,
    string? Color,
    string Country,
    string City,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static VehicleResponse FromEntity(Vehicle vehicle) => new(
        vehicle.Id,
        vehicle.OwnerAuthUserId,
        vehicle.Make,
        vehicle.Model,
        vehicle.Year,
        vehicle.VIN,
        vehicle.Price,
        vehicle.Description,
        vehicle.Mileage,
        vehicle.FuelType,
        vehicle.Transmission,
        vehicle.BodyType,
        vehicle.ListingType,
        vehicle.Status,
        vehicle.Color,
        vehicle.Country,
        vehicle.City,
        vehicle.CreatedAt,
        vehicle.UpdatedAt);
}

public record VehicleSummaryResponse(
    Guid Id,
    Guid OwnerAuthUserId,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    FuelType FuelType,
    TransmissionType Transmission,
    BodyType BodyType,
    ListingType ListingType,
    VehicleStatus Status,
    string? Color,
    string Country,
    string City,
    DateTime CreatedAt)
{
    public static VehicleSummaryResponse FromEntity(Vehicle vehicle) => new(
        vehicle.Id,
        vehicle.OwnerAuthUserId,
        vehicle.Make,
        vehicle.Model,
        vehicle.Year,
        vehicle.Price,
        vehicle.Mileage,
        vehicle.FuelType,
        vehicle.Transmission,
        vehicle.BodyType,
        vehicle.ListingType,
        vehicle.Status,
        vehicle.Color,
        vehicle.Country,
        vehicle.City,
        vehicle.CreatedAt);
}
