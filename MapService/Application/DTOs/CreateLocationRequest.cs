using System.ComponentModel.DataAnnotations;
using MapService.Domain.Enums;

namespace MapService.Application.DTOs;

public record CreateLocationRequest(
    [Required] Guid EntityId,
    [Required] EntityType EntityType,
    [Required, Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")] double Latitude,
    [Required, Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")] double Longitude,
    [StringLength(300)] string? Address,
    [Required, StringLength(100, MinimumLength = 1)] string City,
    [Required, StringLength(100, MinimumLength = 1)] string Country);
