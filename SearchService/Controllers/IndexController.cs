using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchService.Application.DTOs;
using SearchService.Domain.Constants;
using SearchService.Domain.Entities;
using SearchService.Infrastructure.Data;

namespace SearchService.Controllers;

/// <summary>
/// Upsert/remove indexed documents. Owning services (vehicles, providers, ...)
/// call these endpoints to keep the index in sync with their catalogue.
/// </summary>
[ApiController]
[Authorize(Policy = SearchPolicies.IndexManager)]
[Route("api/search/index")]
public class IndexController(SearchDbContext dbContext) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult<IndexedDocumentResponse>> Upsert(
        IndexDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.SearchDocuments
            .FirstOrDefaultAsync(existing => existing.ExternalId == request.ExternalId, cancellationToken);

        var now = DateTime.UtcNow;
        var created = document is null;

        if (document is null)
        {
            document = new SearchDocument
            {
                Id = Guid.NewGuid(),
                ExternalId = request.ExternalId,
                CreatedAt = now
            };
            dbContext.SearchDocuments.Add(document);
        }

        document.Type = request.Type;
        document.Title = request.Title;
        document.Description = request.Description;
        document.Category = request.Category;
        document.Brand = request.Brand;
        document.City = request.City;
        document.Country = request.Country;
        document.Latitude = request.Latitude;
        document.Longitude = request.Longitude;
        document.Price = request.Price;
        document.Currency = request.Currency;
        document.Tags = request.Tags ?? string.Empty;
        document.Popularity = request.Popularity ?? document.Popularity;
        document.IsActive = request.IsActive ?? true;
        document.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(document);
        return created
            ? CreatedAtAction(nameof(GetByExternalId), new { externalId = document.ExternalId }, response)
            : Ok(response);
    }

    [HttpGet("{externalId:guid}")]
    public async Task<ActionResult<IndexedDocumentResponse>> GetByExternalId(
        Guid externalId,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.SearchDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.ExternalId == externalId, cancellationToken);

        return document is null
            ? NotFound(new { message = "Indexed document was not found." })
            : Ok(ToResponse(document));
    }

    [HttpDelete("{externalId:guid}")]
    public async Task<IActionResult> Delete(Guid externalId, CancellationToken cancellationToken)
    {
        var document = await dbContext.SearchDocuments
            .FirstOrDefaultAsync(existing => existing.ExternalId == externalId, cancellationToken);

        if (document is null)
        {
            return NotFound(new { message = "Indexed document was not found." });
        }

        dbContext.SearchDocuments.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static IndexedDocumentResponse ToResponse(SearchDocument document)
    {
        return new IndexedDocumentResponse(
            document.Id,
            document.ExternalId,
            document.Type,
            document.Title,
            document.Category,
            document.City,
            document.Country,
            document.Price,
            document.IsActive,
            document.UpdatedAt);
    }
}
