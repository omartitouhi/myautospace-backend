namespace SearchService.Application.DTOs;

public record SynonymRequest(string Canonical, IReadOnlyList<string> Synonyms);

public record SynonymResponse(
    Guid Id,
    string Canonical,
    IReadOnlyList<string> Synonyms,
    DateTime CreatedAt);
