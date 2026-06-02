namespace SearchService.Application.DTOs;

public record SearchResponse(
    IReadOnlyList<SearchResultItem> Items,
    int Total,
    int Page,
    int PageSize,
    IReadOnlyList<string> ExpandedTerms);
