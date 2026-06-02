namespace SearchService.Domain.Entities;

/// <summary>
/// Every executed query is logged. The log feeds query suggestions
/// (most frequent past terms) and auto-complete.
/// </summary>
public class SearchLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Term { get; set; } = string.Empty;

    public int ResultCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
