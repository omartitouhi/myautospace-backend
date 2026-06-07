using System.ComponentModel.DataAnnotations;
using ProviderService.Domain.Enums;

namespace ProviderService.Application.DTOs;

public record CreateServiceOfferingRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [StringLength(1000)] string? Description,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")] decimal Price,
    [Required, Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")] int DurationMinutes,
    [Required] ServiceCategory Category);
