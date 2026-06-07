using System.ComponentModel.DataAnnotations;

namespace ProviderService.Application.DTOs;

public record SetAvailabilityRequest(
    [Required] DayOfWeek DayOfWeek,
    [Required] TimeOnly StartTime,
    [Required] TimeOnly EndTime,
    bool IsAvailable = true);
