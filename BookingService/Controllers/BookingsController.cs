using BookingService.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly BookingService.Application.Services.BookingService _service;

    public BookingsController(BookingService.Application.Services.BookingService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        // externalCustomerId should usually come from token (sub)
        var externalCustomerId = User?.Identity?.Name ?? request.ExternalCustomerId ?? throw new InvalidOperationException("Missing customer id");
        var dto = await _service.CreateAsync(externalCustomerId, request.ProviderId, request.StartUtc, request.EndUtc, request.Price, request.Metadata);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _service.CancelAsync(id);
        return Ok();
    }
}

public class CreateBookingRequest
{
    public string? ExternalCustomerId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal? Price { get; set; }
    public string? Metadata { get; set; }
}


