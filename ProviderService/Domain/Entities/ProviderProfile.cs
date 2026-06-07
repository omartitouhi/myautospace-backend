using ProviderService.Domain.Enums;

namespace ProviderService.Domain.Entities;

public class ProviderProfile
{
    public Guid Id { get; set; }

    public Guid AuthUserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public ProviderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<ServiceOffering> Services { get; set; } = new List<ServiceOffering>();

    public ICollection<ProviderAvailability> Availabilities { get; set; } = new List<ProviderAvailability>();

    public ICollection<ProviderGallery> GalleryImages { get; set; } = new List<ProviderGallery>();
}
