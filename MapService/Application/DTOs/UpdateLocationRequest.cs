using System.ComponentModel.DataAnnotations;

namespace MapService.Application.DTOs;

public record UpdateLocationRequest(
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")] double? Latitude,
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")] double? Longitude,
    [StringLength(300)] string? Address,
    [StringLength(100, MinimumLength = 1)] string? City,
    [StringLength(100, MinimumLength = 1)] string? Country,
    bool? IsActive);
