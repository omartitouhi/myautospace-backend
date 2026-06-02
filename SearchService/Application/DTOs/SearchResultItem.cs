using SearchService.Domain.Enums;

namespace SearchService.Application.DTOs;

public record SearchResultItem(
    Guid Id,
    Guid ExternalId,
    SearchableType Type,
    string Title,
    string Description,
    string Category,
    string? Brand,
    string City,
    string Country,
    decimal? Price,
    string? Currency,
    double? DistanceKm,
    double Score);
