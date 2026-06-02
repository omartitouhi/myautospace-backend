using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;

namespace VehicleService.Infrastructure.Data;

public class VehicleDbContext(DbContextOptions<VehicleDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.HasIndex(v => v.OwnerAuthUserId);

            entity.HasIndex(v => v.VIN)
                .IsUnique()
                .HasFilter("\"VIN\" IS NOT NULL");

            entity.Property(v => v.Make)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.VIN)
                .HasMaxLength(17);

            entity.Property(v => v.Price)
                .HasColumnType("numeric(18,2)");

            entity.Property(v => v.Description)
                .HasMaxLength(2000);

            entity.Property(v => v.Color)
                .HasMaxLength(50);

            entity.Property(v => v.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.FuelType)
                .HasConversion<string>();

            entity.Property(v => v.Transmission)
                .HasConversion<string>();

            entity.Property(v => v.BodyType)
                .HasConversion<string>();

            entity.Property(v => v.ListingType)
                .HasConversion<string>();

            entity.Property(v => v.Status)
                .HasConversion<string>();

            entity.HasQueryFilter(v => !v.IsDeleted);
        });
    }
}
