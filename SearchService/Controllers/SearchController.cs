using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;

namespace SearchService.Controllers;

[ApiController]
[Authorize]
[Route("api/search")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>Full-text search with filters, sorting and geo proximity.</summary>
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search(
        [FromQuery] SearchRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await searchService.SearchAsync(request, GetUserId(), cancellationToken));
    }

    /// <summary>Query suggestions ("did you mean / popular") for a partial term.</summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<SuggestionResponse>> Suggest(
        [FromQuery] string? term,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await searchService.SuggestAsync(term ?? string.Empty, limit, cancellationToken));
    }

    /// <summary>Prefix auto-complete for the search box.</summary>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<SuggestionResponse>> AutoComplete(
        [FromQuery] string prefix,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Ok(new SuggestionResponse([]));
        }

        return Ok(await searchService.AutoCompleteAsync(prefix, limit, cancellationToken));
    }

    private Guid? GetUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userId, out var parsed) ? parsed : null;
    }
}
