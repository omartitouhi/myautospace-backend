using System.ComponentModel.DataAnnotations;

namespace AdminService.Application.DTOs;

public record UpdateSystemConfigRequest(
    [Required] string Value,
    string? Description,
    bool IsSensitive);
