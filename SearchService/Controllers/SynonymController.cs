using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchService.Application.DTOs;
using SearchService.Domain.Constants;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Data;

namespace SearchService.Controllers;

/// <summary>Admin-managed synonym dictionary used to expand query terms.</summary>
[ApiController]
[Authorize(Policy = SearchPolicies.Admin)]
[Route("api/search/synonyms")]
public class SynonymController(SearchDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SynonymResponse>>> List(CancellationToken cancellationToken)
    {
        var groups = await dbContext.SynonymGroups
            .AsNoTracking()
            .OrderBy(group => group.Canonical)
            .ToListAsync(cancellationToken);

        return Ok(groups.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<SynonymResponse>> Create(SynonymRequest request, CancellationToken cancellationToken)
    {
        var canonical = request.Canonical.Trim().ToLowerInvariant();
        if (canonical.Length == 0)
        {
            return BadRequest(new { message = "Canonical term is required." });
        }

        var exists = await dbContext.SynonymGroups
            .AnyAsync(group => group.Canonical == canonical, cancellationToken);

        if (exists)
        {
            return Conflict(new { message = "A synonym group already exists for this canonical term." });
        }

        var group = new SynonymGroup
        {
            Id = Guid.NewGuid(),
            Canonical = canonical,
            Synonyms = JoinSynonyms(request.Synonyms),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.SynonymGroups.Add(group);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(List), new { id = group.Id }, ToResponse(group));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SynonymResponse>> Update(Guid id, SynonymRequest request, CancellationToken cancellationToken)
    {
        var group = await dbContext.SynonymGroups.FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);
        if (group is null)
        {
            return NotFound(new { message = "Synonym group was not found." });
        }

        group.Canonical = request.Canonical.Trim().ToLowerInvariant();
        group.Synonyms = JoinSynonyms(request.Synonyms);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(group));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var group = await dbContext.SynonymGroups.FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);
        if (group is null)
        {
            return NotFound(new { message = "Synonym group was not found." });
        }

        dbContext.SynonymGroups.Remove(group);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static string JoinSynonyms(IReadOnlyList<string> synonyms)
    {
        return string.Join(',', synonyms
            .Select(synonym => synonym.Trim().ToLowerInvariant())
            .Where(synonym => synonym.Length > 0)
            .Distinct());
    }

    private static SynonymResponse ToResponse(SynonymGroup group)
    {
        var synonyms = group.Synonyms
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new SynonymResponse(group.Id, group.Canonical, synonyms, group.CreatedAt);
    }
}
