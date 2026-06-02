using Microsoft.EntityFrameworkCore;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Data;

public class SearchDbContext(DbContextOptions<SearchDbContext> options) : DbContext(options)
{
    public DbSet<SearchDocument> SearchDocuments { get; set; } = null!;

    public DbSet<SynonymGroup> SynonymGroups { get; set; } = null!;

    public DbSet<SearchLog> SearchLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SearchDocument>()
            .HasIndex(document => document.ExternalId)
            .IsUnique();

        modelBuilder.Entity<SearchDocument>()
            .HasIndex(document => document.Type);

        modelBuilder.Entity<SearchDocument>()
            .HasIndex(document => document.Category);

        modelBuilder.Entity<SearchDocument>()
            .HasIndex(document => document.City);

        modelBuilder.Entity<SynonymGroup>()
            .HasIndex(group => group.Canonical)
            .IsUnique();

        modelBuilder.Entity<SearchLog>()
            .HasIndex(log => log.Term);
    }
}
