using SearchService.Domain.Enums;

namespace SearchService.Application.DTOs;

/// <summary>
/// Full-text query plus filters, sorting and geo proximity. Bound from the query string.
/// </summary>
public record SearchRequest
{
    public string? Query { get; init; }

    public SearchableType? Type { get; init; }

    public string? Category { get; init; }

    public string? Brand { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    // Geo / proximity search.
    public double? Latitude { get; init; }

    public double? Longitude { get; init; }

    public double? RadiusKm { get; init; }

    public SearchSortOrder Sort { get; init; } = SearchSortOrder.Relevance;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
