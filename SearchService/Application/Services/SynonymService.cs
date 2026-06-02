using Microsoft.EntityFrameworkCore;
using SearchService.Application.Interfaces;
using SearchService.Infrastructure.Data;

namespace SearchService.Application.Services;

public class SynonymService(SearchDbContext dbContext) : ISynonymService
{
    public async Task<IReadOnlyCollection<string>> ExpandAsync(
        IEnumerable<string> tokens,
        CancellationToken cancellationToken = default)
    {
        var normalizedTokens = tokens
            .Select(Normalize)
            .Where(token => token.Length > 0)
            .ToHashSet();

        if (normalizedTokens.Count == 0)
        {
            return normalizedTokens;
        }

        var groups = await dbContext.SynonymGroups
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var expanded = new HashSet<string>(normalizedTokens);

        foreach (var group in groups)
        {
            var members = SplitTerms(group.Synonyms)
                .Append(Normalize(group.Canonical))
                .Where(term => term.Length > 0)
                .ToList();

            // If any query token belongs to this group, pull in the whole group.
            if (members.Any(normalizedTokens.Contains))
            {
                foreach (var member in members)
                {
                    expanded.Add(member);
                }
            }
        }

        return expanded;
    }

    private static IEnumerable<string> SplitTerms(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
