using BookingService.Domain.Entities;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(Guid id);
    Task UpdateAsync(Booking booking);
    Task<IEnumerable<Booking>> GetByCustomerAsync(Guid customerId, DateTime? from = null, DateTime? to = null);
}

