using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingHistory> BookingHistories { get; set; } = null!;
    public DbSet<NoShow> NoShows { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.CustomerUserId });

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.ProviderUserId, b.ScheduledAt });

        // One-to-many: Booking -> BookingHistory
        modelBuilder.Entity<Booking>()
            .HasMany(b => b.BookingHistory)
            .WithOne(h => h.Booking)
            .HasForeignKey(h => h.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingHistory>()
            .HasIndex(h => h.BookingId);
    }
}

