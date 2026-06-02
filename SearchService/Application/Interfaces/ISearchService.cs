using SearchService.Application.DTOs;

namespace SearchService.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(SearchRequest request, Guid? userId, CancellationToken cancellationToken = default);

    Task<SuggestionResponse> SuggestAsync(string term, int limit, CancellationToken cancellationToken = default);

    Task<SuggestionResponse> AutoCompleteAsync(string prefix, int limit, CancellationToken cancellationToken = default);
}
