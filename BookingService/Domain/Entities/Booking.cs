using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerUserId { get; set; }

    public Guid ProviderUserId { get; set; }

    public Guid? VehicleId { get; set; }

    public string ServiceType { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; }

    public BookingService.Domain.Enums.BookingStatus Status { get; set; } = BookingService.Domain.Enums.BookingStatus.Pending;

    public string? Note { get; set; }

    // Denormalized vehicle info (resolved from VehicleService at creation) so a
    // bookings list renders without a per-row vehicle fetch.
    public string? VehicleTitle { get; set; }

    public string? VehicleLocation { get; set; }

    public string? CancellationReason { get; set; }

    public Guid? RescheduledFromBookingId { get; set; }

    public string? CheckInCode { get; set; }

    public string? QrCodePayload { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation: one booking has many history entries
    public List<BookingHistory> BookingHistory { get; set; } = new();
}
