using SearchService.Domain.Enums;

namespace SearchService.Domain.Entities;

/// <summary>
/// A denormalized, indexable projection of a listing owned by another service
/// (a vehicle, a service listing or a provider). Other services push these
/// documents into the search index; SearchService never owns the source data.
/// </summary>
public class SearchDocument
{
    public Guid Id { get; set; }

    /// <summary>Identifier of the source entity in its owning service.</summary>
    public Guid ExternalId { get; set; }

    public SearchableType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public decimal? Price { get; set; }

    public string? Currency { get; set; }

    /// <summary>Comma-separated keywords, weighted into full-text matching.</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Editorial / behavioural boost used by ranking.</summary>
    public int Popularity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
