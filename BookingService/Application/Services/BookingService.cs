using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;

namespace BookingService.Application.Services;

public class BookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto> CreateAsync(Guid customerUserId, Guid providerUserId, Guid? vehicleId, string serviceType, DateTime scheduledAt, int durationMinutes)
    {
        var booking = new Booking
        {
            CustomerUserId = customerUserId,
            ProviderUserId = providerUserId,
            VehicleId = vehicleId,
            ServiceType = serviceType,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(booking);

        return new BookingDto(created.Id, created.CustomerUserId, created.ProviderUserId, created.VehicleId, created.ServiceType, created.ScheduledAt, created.DurationMinutes, created.Status.ToString());
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id)
    {
        var booking = await _repository.GetByIdAsync(id);
        if (booking is null) return null;
        return new BookingDto(booking.Id, booking.CustomerUserId, booking.ProviderUserId, booking.VehicleId, booking.ServiceType, booking.ScheduledAt, booking.DurationMinutes, booking.Status.ToString());
    }

    public async Task CancelAsync(Guid id, string? reason = null)
    {
        var booking = await _repository.GetByIdAsync(id) ?? throw new InvalidOperationException("Booking not found");
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(booking);
    }
}

