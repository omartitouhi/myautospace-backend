using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record UpdateVehicleRequest(
    [StringLength(100, MinimumLength = 1)] string? Make,
    [StringLength(100, MinimumLength = 1)] string? Model,
    [Range(1900, 2100)] int? Year,
    [StringLength(17, MinimumLength = 17)] string? VIN,
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal? Price,
    [StringLength(2000)] string? Description,
    [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")] int? Mileage,
    FuelType? FuelType,
    TransmissionType? Transmission,
    BodyType? BodyType,
    [StringLength(50)] string? Color,
    [StringLength(100, MinimumLength = 1)] string? Country,
    [StringLength(100, MinimumLength = 1)] string? City);
