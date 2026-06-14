using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public class BookingRepository(BookingDbContext db) : IBookingRepository
{
    public async Task<Booking> AddAsync(Booking booking)
    {
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateAsync(Booking booking, BookingHistory? history = null)
    {
        // `booking` is tracked (loaded via GetByIdAsync), so its mutations
        // persist on SaveChanges. Add any history row directly to the set
        // rather than through the (unloaded) navigation collection, which would
        // trigger EF's orphan handling against rows it never loaded.
        _ = booking;
        if (history is not null)
        {
            db.BookingHistories.Add(history);
        }

        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Booking>> GetByCustomerAsync(Guid customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = db.Bookings.AsNoTracking().Where(b => b.CustomerUserId == customerId);
        if (from.HasValue) query = query.Where(b => b.ScheduledAt >= from.Value);
        if (to.HasValue) query = query.Where(b => b.ScheduledAt <= to.Value);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByProviderAsync(Guid providerId, DateTime? from = null, DateTime? to = null)
    {
        var query = db.Bookings.AsNoTracking().Where(b => b.ProviderUserId == providerId);
        if (from.HasValue) query = query.Where(b => b.ScheduledAt >= from.Value);
        if (to.HasValue) query = query.Where(b => b.ScheduledAt <= to.Value);
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }
}
