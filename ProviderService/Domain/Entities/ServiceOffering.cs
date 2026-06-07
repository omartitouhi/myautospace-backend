using ProviderService.Domain.Enums;

namespace ProviderService.Domain.Entities;

public class ServiceOffering
{
    public Guid Id { get; set; }

    public Guid ProviderProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public ServiceCategory Category { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ProviderProfile ProviderProfile { get; set; } = null!;
}
