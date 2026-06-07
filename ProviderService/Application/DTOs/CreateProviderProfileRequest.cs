using System.ComponentModel.DataAnnotations;
using ProviderService.Domain.Enums;
using ProviderService.Domain.Entities;

namespace ProviderService.Application.DTOs;

public record CreateProviderProfileRequest(
    [Required, StringLength(200, MinimumLength = 2)] string BusinessName,
    [StringLength(2000)] string? Description,
    [StringLength(30)] string? PhoneNumber,
    [StringLength(300)] string? Address,
    [Required, StringLength(100, MinimumLength = 1)] string City,
    [Required, StringLength(100, MinimumLength = 1)] string Country);
