namespace BookingService.Domain.Enums;

public enum BookingAction
{
    Created = 0,
    Updated = 1,
    StatusChanged = 2,
    Cancelled = 3,
    Rescheduled = 4,
    CheckIn = 5,
    NoShow = 6
}

