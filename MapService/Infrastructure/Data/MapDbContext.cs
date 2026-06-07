using Microsoft.EntityFrameworkCore;
using MapService.Domain.Entities;

namespace MapService.Infrastructure.Data;

public class MapDbContext(DbContextOptions<MapDbContext> options) : DbContext(options)
{
    public DbSet<MapLocation> MapLocations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MapLocation>(entity =>
        {
            entity.HasKey(l => l.Id);

            // One entity (Vehicle/Provider) has at most one active location per type
            entity.HasIndex(l => new { l.EntityId, l.EntityType })
                .IsUnique();

            entity.HasIndex(l => l.OwnerAuthUserId);

            // Composite index for geo bounding-box queries filtered by type
            entity.HasIndex(l => new { l.EntityType, l.Latitude, l.Longitude });

            entity.Property(l => l.Address)
                .HasMaxLength(300);

            entity.Property(l => l.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(l => l.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(l => l.EntityType)
                .HasConversion<string>();
        });
    }
}
