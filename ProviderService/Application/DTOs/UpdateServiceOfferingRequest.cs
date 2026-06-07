using System.ComponentModel.DataAnnotations;
using ProviderService.Domain.Enums;

namespace ProviderService.Application.DTOs;

public record UpdateServiceOfferingRequest(
    [StringLength(200, MinimumLength = 1)] string? Name,
    [StringLength(1000)] string? Description,
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal? Price,
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")] int? DurationMinutes,
    ServiceCategory? Category,
    bool? IsActive);
