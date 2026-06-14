using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController]
[Authorize(Policy = BookingPolicies.AuthenticatedUser)]
[Route("api/bookings")]
public class BookingsController(
    Application.Services.BookingService bookingService,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>Buyer requests a booking (test drive / rental) for a vehicle.</summary>
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            var response = await bookingService.CreateAsync(user, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Bookings the current user made (as buyer).</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyCollection<BookingResponse>>> GetMy()
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        return Ok(await bookingService.GetMyAsync(user.UserId));
    }

    /// <summary>Booking requests for vehicles the current user owns (as seller).</summary>
    [HttpGet("incoming")]
    public async Task<ActionResult<IReadOnlyCollection<BookingResponse>>> GetIncoming()
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        return Ok(await bookingService.GetIncomingAsync(user.UserId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            return Ok(await bookingService.GetByIdAsync(id, user.UserId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Confirm / complete / cancel a booking (role-checked).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(Guid id, UpdateBookingStatusRequest request)
    {
        var user = currentUserService.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            return Ok(await bookingService.UpdateStatusAsync(id, request, user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
