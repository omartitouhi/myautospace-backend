using System.ComponentModel.DataAnnotations;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;

namespace BookingService.Application.DTOs;

/// <summary>A buyer requests a booking against a vehicle they don't own.</summary>
public record CreateBookingRequest(
    [Required] Guid VehicleId,
    [Required, StringLength(50, MinimumLength = 1)] string ServiceType,
    [Required] DateTime ScheduledAt,
    [Range(5, 1440 * 30)] int DurationMinutes,
    [StringLength(500)] string? Note);

public record UpdateBookingStatusRequest(
    [Required] BookingStatus Status,
    [StringLength(300)] string? Reason);

public record BookingResponse(
    Guid Id,
    Guid? VehicleId,
    Guid CustomerUserId,
    Guid ProviderUserId,
    string ServiceType,
    DateTime ScheduledAt,
    int DurationMinutes,
    BookingStatus Status,
    string? Note,
    string? VehicleTitle,
    string? VehicleLocation,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static BookingResponse FromEntity(Booking booking) => new(
        booking.Id,
        booking.VehicleId,
        booking.CustomerUserId,
        booking.ProviderUserId,
        booking.ServiceType,
        booking.ScheduledAt,
        booking.DurationMinutes,
        booking.Status,
        booking.Note,
        booking.VehicleTitle,
        booking.VehicleLocation,
        booking.CancellationReason,
        booking.CreatedAt,
        booking.UpdatedAt);
}
