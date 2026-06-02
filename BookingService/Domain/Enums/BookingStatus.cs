namespace BookingService.Domain.Enums;

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    CheckedIn = 2,
    Completed = 3,
    Cancelled = 4,
    Rescheduled = 5,
    NoShow = 6,
}

