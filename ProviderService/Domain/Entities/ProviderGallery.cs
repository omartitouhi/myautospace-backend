namespace ProviderService.Domain.Entities;

public class ProviderGallery
{
    public Guid Id { get; set; }

    public Guid ProviderProfileId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime UploadedAt { get; set; }

    public ProviderProfile ProviderProfile { get; set; } = null!;
}
