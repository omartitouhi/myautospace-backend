using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record CreateVehicleRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Make,
    [Required, StringLength(100, MinimumLength = 1)] string Model,
    [Required, Range(1900, 2100)] int Year,
    [StringLength(17, MinimumLength = 17)] string? VIN,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal Price,
    [StringLength(2000)] string? Description,
    [Required, Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")] int Mileage,
    [Required] FuelType FuelType,
    [Required] TransmissionType Transmission,
    [Required] BodyType BodyType,
    [Required] ListingType ListingType,
    [StringLength(50)] string? Color,
    [Required, StringLength(100, MinimumLength = 1)] string Country,
    [Required, StringLength(100, MinimumLength = 1)] string City);
