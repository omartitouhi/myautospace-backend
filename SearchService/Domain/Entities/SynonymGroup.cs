namespace SearchService.Domain.Entities;

/// <summary>
/// A canonical term and its equivalent expressions. Query tokens are expanded
/// through these groups so that "car" also matches "auto", "vehicle", etc.
/// </summary>
public class SynonymGroup
{
    public Guid Id { get; set; }

    public string Canonical { get; set; } = string.Empty;

    /// <summary>Comma-separated list of equivalent terms.</summary>
    public string Synonyms { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
