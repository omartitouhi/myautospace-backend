using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BookingService.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // External user identifier (sub claim from AuthService)
    public string ExternalCustomerId { get; set; } = null!;

    public Guid ProviderId { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public BookingService.Domain.Enums.BookingStatus Status { get; set; } = BookingService.Domain.Enums.BookingStatus.Pending;

    public decimal? Price { get; set; }

    public string? Metadata { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public string? QrCodeToken { get; set; }
}


