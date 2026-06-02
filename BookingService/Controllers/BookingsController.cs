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
        // customerUserId should usually come from token (sub)
        Guid customerUserId;
        if (User?.Identity?.Name is string name && Guid.TryParse(name, out var parsed))
        {
            customerUserId = parsed;
        }
        else if (request.CustomerUserId.HasValue)
        {
            customerUserId = request.CustomerUserId.Value;
        }
        else
        {
            throw new InvalidOperationException("Missing customer id");
        }

        var dto = await _service.CreateAsync(customerUserId, request.ProviderUserId, request.VehicleId, request.ServiceType, request.ScheduledAt, request.DurationMinutes);
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
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest? request)
    {
        await _service.CancelAsync(id, request?.Reason);
        return Ok();
    }
}

public class CreateBookingRequest
{
    public Guid? CustomerUserId { get; set; }
    public Guid ProviderUserId { get; set; }
    public Guid? VehicleId { get; set; }
    public string ServiceType { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
}

public class CancelRequest
{
    public string? Reason { get; set; }
}


