using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record UpdateVehicleRequest(
    [property: StringLength(100, MinimumLength = 1)] string? Make,
    [property: StringLength(100, MinimumLength = 1)] string? Model,
    [property: Range(1900, 2100)] int? Year,
    [property: StringLength(17, MinimumLength = 17)] string? VIN,
    [property: Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal? Price,
    [property: StringLength(2000)] string? Description,
    [property: Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")] int? Mileage,
    FuelType? FuelType,
    TransmissionType? Transmission,
    BodyType? BodyType,
    [property: StringLength(50)] string? Color,
    [property: StringLength(100, MinimumLength = 1)] string? Country,
    [property: StringLength(100, MinimumLength = 1)] string? City);
