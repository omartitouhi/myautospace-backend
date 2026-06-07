using Microsoft.EntityFrameworkCore;
using ProviderService.Domain.Entities;
using ProviderService.Domain.Enums;

namespace ProviderService.Infrastructure.Data;

public class ProviderDbContext(DbContextOptions<ProviderDbContext> options) : DbContext(options)
{
    public DbSet<ProviderProfile> ProviderProfiles { get; set; } = null!;
    public DbSet<ServiceOffering> ServiceOfferings { get; set; } = null!;
    public DbSet<ProviderAvailability> ProviderAvailabilities { get; set; } = null!;
    public DbSet<ProviderGallery> ProviderGalleries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── ProviderProfile ──────────────────────────────────────────────────
        modelBuilder.Entity<ProviderProfile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.AuthUserId)
                .IsUnique();

            entity.Property(p => p.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(p => p.Address)
                .HasMaxLength(300);

            entity.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Status)
                .HasConversion<string>();

            entity.HasMany(p => p.Services)
                .WithOne(s => s.ProviderProfile)
                .HasForeignKey(s => s.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Availabilities)
                .WithOne(a => a.ProviderProfile)
                .HasForeignKey(a => a.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.GalleryImages)
                .WithOne(g => g.ProviderProfile)
                .HasForeignKey(g => g.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ServiceOffering ──────────────────────────────────────────────────
        modelBuilder.Entity<ServiceOffering>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.ProviderProfileId);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.Description)
                .HasMaxLength(1000);

            entity.Property(s => s.Price)
                .HasColumnType("numeric(18,2)");

            entity.Property(s => s.Category)
                .HasConversion<string>();
        });

        // ── ProviderAvailability ─────────────────────────────────────────────
        modelBuilder.Entity<ProviderAvailability>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasIndex(a => new { a.ProviderProfileId, a.DayOfWeek });
        });

        // ── ProviderGallery ──────────────────────────────────────────────────
        modelBuilder.Entity<ProviderGallery>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.HasIndex(g => g.ProviderProfileId);

            entity.Property(g => g.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(g => g.Caption)
                .HasMaxLength(200);
        });
    }
}
