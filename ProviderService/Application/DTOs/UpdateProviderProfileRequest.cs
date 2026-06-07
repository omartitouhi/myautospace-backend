using System.ComponentModel.DataAnnotations;

namespace ProviderService.Application.DTOs;

public record UpdateProviderProfileRequest(
    [StringLength(200, MinimumLength = 2)] string? BusinessName,
    [StringLength(2000)] string? Description,
    [StringLength(30)] string? PhoneNumber,
    [StringLength(300)] string? Address,
    [StringLength(100, MinimumLength = 1)] string? City,
    [StringLength(100, MinimumLength = 1)] string? Country);
