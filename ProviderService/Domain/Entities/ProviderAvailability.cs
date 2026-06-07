namespace ProviderService.Domain.Entities;

public class ProviderAvailability
{
    public Guid Id { get; set; }

    public Guid ProviderProfileId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public ProviderProfile ProviderProfile { get; set; } = null!;
}
