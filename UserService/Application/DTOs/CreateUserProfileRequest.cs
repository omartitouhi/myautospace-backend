using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs;

public record CreateUserProfileRequest(
    [Required] string FirstName,
    [Required] string LastName,
    DateOnly? BirthDate,
    string? PhoneNumber,
    string? Address,
    string? Country,
    string? City,
    string? ProfilePictureUrl,
    string? Bio);
