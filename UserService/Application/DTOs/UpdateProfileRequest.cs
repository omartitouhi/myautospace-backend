namespace UserService.Application.DTOs;

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    DateOnly? BirthDate,
    string? PhoneNumber,
    string? Address,
    string? Country,
    string? City,
    string? ProfilePictureUrl,
    string? Bio);
