namespace SearchService.Application.Interfaces;

public interface ISynonymService
{
    /// <summary>
    /// Expands a set of query tokens with their configured synonyms, returning the
    /// distinct, lower-cased union of the originals and every equivalent term.
    /// </summary>
    Task<IReadOnlyCollection<string>> ExpandAsync(IEnumerable<string> tokens, CancellationToken cancellationToken = default);
}
