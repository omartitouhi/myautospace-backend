using Microsoft.EntityFrameworkCore;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;
using SearchService.Domain.Enums;
using SearchService.Infrastructure.Data;

namespace SearchService.Application.Services;

public class SearchService(SearchDbContext dbContext, ISynonymService synonymService) : ISearchService
{
    private const int MaxPageSize = 100;
    private const double EarthRadiusKm = 6371.0;
    private const double KmPerDegreeLatitude = 111.0;

    public async Task<SearchResponse> SearchAsync(
        SearchRequest request,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var tokens = Tokenize(request.Query);
        var expandedTerms = (await synonymService.ExpandAsync(tokens, cancellationToken)).ToList();

        // Structured filters run in SQL; text scoring & geo ranking run in memory.
        var query = dbContext.SearchDocuments
            .AsNoTracking()
            .Where(document => document.IsActive);

        if (request.Type is { } type)
        {
            query = query.Where(document => document.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(document => document.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            query = query.Where(document => document.Brand == request.Brand);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(document => document.City == request.City);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            query = query.Where(document => document.Country == request.Country);
        }

        if (request.MinPrice is { } minPrice)
        {
            query = query.Where(document => document.Price != null && document.Price >= minPrice);
        }

        if (request.MaxPrice is { } maxPrice)
        {
            query = query.Where(document => document.Price != null && document.Price <= maxPrice);
        }

        var geo = TryBuildGeoFilter(request);
        if (geo is { } box)
        {
            query = query.Where(document =>
                document.Latitude != null && document.Longitude != null
                && document.Latitude >= box.MinLat && document.Latitude <= box.MaxLat
                && document.Longitude >= box.MinLon && document.Longitude <= box.MaxLon);
        }

        var candidates = await query.ToListAsync(cancellationToken);

        var scored = candidates
            .Select(document => Evaluate(document, expandedTerms, request))
            .Where(result => result is not null)
            .Select(result => result!)
            .ToList();

        var ordered = Order(scored, request.Sort);
        var total = ordered.Count;

        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(scoredDocument => scoredDocument.Item)
            .ToList();

        await LogSearchAsync(request.Query, userId, total, cancellationToken);

        return new SearchResponse(items, total, page, pageSize, expandedTerms);
    }

    public async Task<SuggestionResponse> SuggestAsync(
        string term,
        int limit,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 25);
        var normalized = term.Trim();

        if (normalized.Length == 0)
        {
            // Fall back to the globally most popular past queries.
            var popular = await dbContext.SearchLogs
                .AsNoTracking()
                .GroupBy(log => log.Term)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return new SuggestionResponse(popular);
        }

        var pattern = $"%{normalized}%";

        var fromHistory = await dbContext.SearchLogs
            .AsNoTracking()
            .Where(log => EF.Functions.ILike(log.Term, pattern))
            .GroupBy(log => log.Term)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var fromTitles = await dbContext.SearchDocuments
            .AsNoTracking()
            .Where(document => document.IsActive && EF.Functions.ILike(document.Title, pattern))
            .OrderByDescending(document => document.Popularity)
            .Select(document => document.Title)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var suggestions = fromHistory
            .Concat(fromTitles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();

        return new SuggestionResponse(suggestions);
    }

    public async Task<SuggestionResponse> AutoCompleteAsync(
        string prefix,
        int limit,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 25);
        var normalized = prefix.Trim();

        if (normalized.Length == 0)
        {
            return new SuggestionResponse([]);
        }

        var pattern = $"{normalized}%";

        var titles = await dbContext.SearchDocuments
            .AsNoTracking()
            .Where(document => document.IsActive && EF.Functions.ILike(document.Title, pattern))
            .OrderByDescending(document => document.Popularity)
            .Select(document => document.Title)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var canonicalTerms = await dbContext.SynonymGroups
            .AsNoTracking()
            .Where(group => EF.Functions.ILike(group.Canonical, pattern))
            .Select(group => group.Canonical)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var pastTerms = await dbContext.SearchLogs
            .AsNoTracking()
            .Where(log => EF.Functions.ILike(log.Term, pattern))
            .GroupBy(log => log.Term)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var suggestions = titles
            .Concat(canonicalTerms)
            .Concat(pastTerms)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();

        return new SuggestionResponse(suggestions);
    }

    private ScoredDocument? Evaluate(SearchDocument document, IReadOnlyCollection<string> terms, SearchRequest request)
    {
        double textScore = 0;

        if (terms.Count > 0)
        {
            foreach (var term in terms)
            {
                textScore += FieldWeight(document.Title, term, weight: 5);
                textScore += FieldWeight(document.Brand, term, weight: 4);
                textScore += FieldWeight(document.Tags, term, weight: 3);
                textScore += FieldWeight(document.Category, term, weight: 2);
                textScore += FieldWeight(document.Description, term, weight: 1);
            }

            // A document that matches none of the (expanded) terms is not a result.
            if (textScore == 0)
            {
                return null;
            }
        }

        double? distanceKm = null;
        if (request is { Latitude: { } lat, Longitude: { } lon }
            && document is { Latitude: { } docLat, Longitude: { } docLon })
        {
            distanceKm = Haversine(lat, lon, docLat, docLon);
        }

        // Ranking blends text relevance, popularity, recency and proximity.
        var recencyBoost = Math.Max(0, 30 - (DateTime.UtcNow - document.UpdatedAt).TotalDays) / 30.0;
        var proximityBoost = distanceKm is { } d ? 1.0 / (1.0 + d) : 0;
        var score = textScore + (document.Popularity * 0.1) + recencyBoost + proximityBoost;

        var item = new SearchResultItem(
            document.Id,
            document.ExternalId,
            document.Type,
            document.Title,
            document.Description,
            document.Category,
            document.Brand,
            document.City,
            document.Country,
            document.Price,
            document.Currency,
            distanceKm,
            Math.Round(score, 4));

        return new ScoredDocument(item, score, document);
    }

    private static List<ScoredDocument> Order(List<ScoredDocument> scored, SearchSortOrder sort)
    {
        return sort switch
        {
            SearchSortOrder.PriceAscending => scored
                .OrderBy(s => s.Document.Price ?? decimal.MaxValue)
                .ThenByDescending(s => s.Score)
                .ToList(),
            SearchSortOrder.PriceDescending => scored
                .OrderByDescending(s => s.Document.Price ?? decimal.MinValue)
                .ThenByDescending(s => s.Score)
                .ToList(),
            SearchSortOrder.Newest => scored
                .OrderByDescending(s => s.Document.CreatedAt)
                .ToList(),
            SearchSortOrder.Distance => scored
                .OrderBy(s => s.Item.DistanceKm ?? double.MaxValue)
                .ThenByDescending(s => s.Score)
                .ToList(),
            _ => scored
                .OrderByDescending(s => s.Score)
                .ThenByDescending(s => s.Document.Popularity)
                .ToList()
        };
    }

    private async Task LogSearchAsync(string? term, Guid? userId, int resultCount, CancellationToken cancellationToken)
    {
        var normalized = term?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return;
        }

        dbContext.SearchLogs.Add(new SearchLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Term = normalized,
            ResultCount = resultCount,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static GeoBox? TryBuildGeoFilter(SearchRequest request)
    {
        if (request is not { Latitude: { } lat, Longitude: { } lon, RadiusKm: { } radius } || radius <= 0)
        {
            return null;
        }

        var deltaLat = radius / KmPerDegreeLatitude;
        var cosLat = Math.Cos(DegreesToRadians(lat));
        var deltaLon = Math.Abs(cosLat) < 1e-6
            ? 180
            : radius / (KmPerDegreeLatitude * Math.Abs(cosLat));

        return new GeoBox(lat - deltaLat, lat + deltaLat, lon - deltaLon, lon + deltaLon);
    }

    private static double FieldWeight(string? field, string term, int weight)
    {
        if (string.IsNullOrEmpty(field))
        {
            return 0;
        }

        return field.Contains(term, StringComparison.OrdinalIgnoreCase) ? weight : 0;
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return EarthRadiusKm * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static List<string> Tokenize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return query
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => new string(token.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant())
            .Where(token => token.Length >= 2)
            .Distinct()
            .ToList();
    }

    private readonly record struct GeoBox(double MinLat, double MaxLat, double MinLon, double MaxLon);

    private sealed record ScoredDocument(SearchResultItem Item, double Score, SearchDocument Document);
}
