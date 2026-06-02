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

    public async Task<BookingDto> CreateAsync(string externalCustomerId, Guid providerId, DateTime startUtc, DateTime endUtc, decimal? price = null, string? metadata = null)
    {
        var booking = new Booking
        {
            ExternalCustomerId = externalCustomerId,
            ProviderId = providerId,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Price = price,
            Metadata = metadata,
            Status = BookingStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(booking);

        return new BookingDto(created.Id, created.ExternalCustomerId, created.ProviderId, created.StartUtc, created.EndUtc, created.Status.ToString());
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id)
    {
        var booking = await _repository.GetByIdAsync(id);
        if (booking is null) return null;
        return new BookingDto(booking.Id, booking.ExternalCustomerId, booking.ProviderId, booking.StartUtc, booking.EndUtc, booking.Status.ToString());
    }

    public async Task CancelAsync(Guid id)
    {
        var booking = await _repository.GetByIdAsync(id) ?? throw new InvalidOperationException("Booking not found");
        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(booking);
    }
}

