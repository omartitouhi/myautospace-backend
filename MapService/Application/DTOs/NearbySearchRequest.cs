using System.ComponentModel.DataAnnotations;
using MapService.Domain.Enums;

namespace MapService.Application.DTOs;

public record NearbySearchRequest
{
    [Required, Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; init; }

    [Required, Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; init; }

    [Required, Range(0.1, 500.0, ErrorMessage = "RadiusKm must be between 0.1 and 500.")]
    public double RadiusKm { get; init; }

    public EntityType? EntityType { get; init; }

    [Range(1, 100)]
    public int Limit { get; init; } = 20;
}
