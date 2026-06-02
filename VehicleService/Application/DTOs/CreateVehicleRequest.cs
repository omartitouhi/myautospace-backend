using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record CreateVehicleRequest(
    [property: Required] string Make,
    [property: Required] string Model,
    [property: Required] int Year,
    string? VIN,
    [property: Required] decimal Price,
    string? Description,
    [property: Required] int Mileage,
    [property: Required] FuelType FuelType,
    [property: Required] TransmissionType Transmission,
    [property: Required] BodyType BodyType,
    [property: Required] ListingType ListingType,
    string? Color,
    [property: Required] string Country,
    [property: Required] string City);
