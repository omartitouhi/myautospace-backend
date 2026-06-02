using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    [Required] UserRoleType? Role);
