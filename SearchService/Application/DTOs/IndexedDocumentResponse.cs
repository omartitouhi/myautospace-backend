using SearchService.Domain.Enums;

namespace SearchService.Application.DTOs;

public record IndexedDocumentResponse(
    Guid Id,
    Guid ExternalId,
    SearchableType Type,
    string Title,
    string Category,
    string City,
    string Country,
    decimal? Price,
    bool IsActive,
    DateTime UpdatedAt);
