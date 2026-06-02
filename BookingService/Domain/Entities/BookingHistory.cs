namespace BookingService.Domain.Entities;

public class BookingHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BookingId { get; set; }

    public BookingService.Domain.Enums.BookingAction Action { get; set; }

    public BookingService.Domain.Enums.BookingStatus? OldStatus { get; set; }

    public BookingService.Domain.Enums.BookingStatus? NewStatus { get; set; }

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedByUserId { get; set; }

    // Navigation
    public Booking? Booking { get; set; }
}


