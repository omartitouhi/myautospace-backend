namespace SearchService.Application.DTOs;

public record SuggestionResponse(IReadOnlyList<string> Suggestions);
