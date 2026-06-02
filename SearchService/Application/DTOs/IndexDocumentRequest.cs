using SearchService.Domain.Enums;

namespace SearchService.Application.DTOs;

/// <summary>
/// Upsert payload pushed by an owning service to (re)index one of its listings.
/// The document is matched/updated by <see cref="ExternalId"/>.
/// </summary>
public record IndexDocumentRequest(
    Guid ExternalId,
    SearchableType Type,
    string Title,
    string Description,
    string Category,
    string? Brand,
    string City,
    string Country,
    double? Latitude,
    double? Longitude,
    decimal? Price,
    string? Currency,
    string? Tags,
    int? Popularity,
    bool? IsActive);
