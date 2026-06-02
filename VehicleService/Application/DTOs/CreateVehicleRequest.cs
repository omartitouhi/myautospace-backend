using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record CreateVehicleRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Make,
    [property: Required, StringLength(100, MinimumLength = 1)] string Model,
    [property: Required, Range(1900, 2100)] int Year,
    [property: StringLength(17, MinimumLength = 17)] string? VIN,
    [property: Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal Price,
    [property: StringLength(2000)] string? Description,
    [property: Required, Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")] int Mileage,
    [property: Required] FuelType FuelType,
    [property: Required] TransmissionType Transmission,
    [property: Required] BodyType BodyType,
    [property: Required] ListingType ListingType,
    [property: StringLength(50)] string? Color,
    [property: Required, StringLength(100, MinimumLength = 1)] string Country,
    [property: Required, StringLength(100, MinimumLength = 1)] string City);
