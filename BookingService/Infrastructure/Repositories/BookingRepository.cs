using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _db;

    public BookingRepository(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateAsync(Booking booking)
    {
        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Booking>> GetByCustomerAsync(Guid customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Bookings.AsQueryable().Where(b => b.CustomerUserId == customerId);
        if (from.HasValue) query = query.Where(b => b.ScheduledAt >= from.Value);
        if (to.HasValue) query = query.Where(b => b.ScheduledAt <= to.Value);
        return await query.ToListAsync();
    }
}

