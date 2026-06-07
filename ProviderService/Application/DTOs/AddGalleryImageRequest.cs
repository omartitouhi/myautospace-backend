using System.ComponentModel.DataAnnotations;

namespace ProviderService.Application.DTOs;

public record AddGalleryImageRequest(
    [Required, StringLength(500, MinimumLength = 1)] string ImageUrl,
    [StringLength(200)] string? Caption,
    [Range(0, int.MaxValue)] int DisplayOrder = 0);
