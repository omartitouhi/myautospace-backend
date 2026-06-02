using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record UpdateVehicleRequest(
    string? Make,
    string? Model,
    int? Year,
    string? VIN,
    decimal? Price,
    string? Description,
    int? Mileage,
    FuelType? FuelType,
    TransmissionType? Transmission,
    BodyType? BodyType,
    string? Color,
    string? Country,
    string? City);
