using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Application.Security;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;

namespace BookingService.Application.Services;

public class BookingService(IBookingRepository repository, IVehicleLookupClient vehicleLookup)
{
    public async Task<BookingResponse> CreateAsync(CurrentUser user, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleLookup.GetVehicleAsync(request.VehicleId, cancellationToken)
            ?? throw new KeyNotFoundException("Vehicle was not found.");

        if (vehicle.OwnerAuthUserId == user.UserId)
        {
            throw new InvalidOperationException("You cannot book your own vehicle.");
        }

        if (!string.Equals(vehicle.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This vehicle is not available for booking.");
        }

        var now = DateTime.UtcNow;
        var scheduledAt = ToUtc(request.ScheduledAt);
        if (scheduledAt <= now)
        {
            throw new InvalidOperationException("The requested time must be in the future.");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerUserId = user.UserId,
            ProviderUserId = vehicle.OwnerAuthUserId,
            VehicleId = vehicle.Id,
            ServiceType = request.ServiceType.Trim(),
            ScheduledAt = scheduledAt,
            DurationMinutes = request.DurationMinutes,
            Status = BookingStatus.Pending,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            VehicleTitle = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
            VehicleLocation = $"{vehicle.City}, {vehicle.Country}",
            CreatedAt = now,
            UpdatedAt = now
        };

        booking.BookingHistory.Add(History(booking.Id, user.UserId, BookingAction.Created, null, BookingStatus.Pending, "Booking requested."));

        var created = await repository.AddAsync(booking);
        return BookingResponse.FromEntity(created);
    }

    public async Task<BookingResponse> GetByIdAsync(Guid id, Guid currentUserId)
    {
        var booking = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Booking was not found.");

        EnsureParticipant(booking, currentUserId);
        return BookingResponse.FromEntity(booking);
    }

    public async Task<IReadOnlyCollection<BookingResponse>> GetMyAsync(Guid customerId)
    {
        var bookings = await repository.GetByCustomerAsync(customerId);
        return bookings.Select(BookingResponse.FromEntity).ToList();
    }

    public async Task<IReadOnlyCollection<BookingResponse>> GetIncomingAsync(Guid providerId)
    {
        var bookings = await repository.GetByProviderAsync(providerId);
        return bookings.Select(BookingResponse.FromEntity).ToList();
    }

    public async Task<BookingResponse> UpdateStatusAsync(Guid id, UpdateBookingStatusRequest request, CurrentUser user)
    {
        var booking = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Booking was not found.");

        var isProvider = booking.ProviderUserId == user.UserId;
        var isCustomer = booking.CustomerUserId == user.UserId;
        if (!isProvider && !isCustomer)
        {
            throw new UnauthorizedAccessException("You are not part of this booking.");
        }

        ValidateTransition(booking.Status, request.Status, isProvider, isCustomer);

        var old = booking.Status;
        booking.Status = request.Status;
        booking.UpdatedAt = DateTime.UtcNow;
        if (request.Status == BookingStatus.Cancelled)
        {
            booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        }

        var history = History(
            booking.Id, user.UserId, BookingAction.StatusChanged, old, request.Status,
            $"Status changed from {old} to {request.Status}.");

        await repository.UpdateAsync(booking, history);
        return BookingResponse.FromEntity(booking);
    }

    // Provider may Confirm/Complete/Cancel; customer may Cancel — only from
    // states where the transition makes sense.
    private static void ValidateTransition(BookingStatus current, BookingStatus target, bool isProvider, bool isCustomer)
    {
        var allowed = target switch
        {
            BookingStatus.Confirmed => isProvider && current == BookingStatus.Pending,
            BookingStatus.Completed => isProvider && current is BookingStatus.Confirmed or BookingStatus.CheckedIn,
            BookingStatus.Cancelled => (isProvider || isCustomer)
                                       && current is BookingStatus.Pending or BookingStatus.Confirmed,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException($"Cannot change a '{current}' booking to '{target}'.");
        }
    }

    private static void EnsureParticipant(Booking booking, Guid userId)
    {
        if (booking.CustomerUserId != userId && booking.ProviderUserId != userId)
        {
            throw new UnauthorizedAccessException("You are not part of this booking.");
        }
    }

    private static BookingHistory History(Guid bookingId, Guid byUserId, BookingAction action, BookingStatus? oldStatus, BookingStatus? newStatus, string description) => new()
    {
        Id = Guid.NewGuid(),
        BookingId = bookingId,
        Action = action,
        OldStatus = oldStatus,
        NewStatus = newStatus,
        Description = description,
        CreatedByUserId = byUserId,
        CreatedAt = DateTime.UtcNow
    };

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
