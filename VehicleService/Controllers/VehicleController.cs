using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleService.Application.DTOs;
using VehicleService.Application.Interfaces;
using VehicleService.Domain.Constants;

namespace VehicleService.Controllers;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public class VehicleController(IVehicleService vehicleService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = VehiclePolicies.Seller)]
    public async Task<ActionResult<VehicleResponse>> Create(CreateVehicleRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            var response = await vehicleService.CreateAsync(request, currentUser.UserId);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = VehiclePolicies.AuthenticatedUser)]
    public async Task<ActionResult<IReadOnlyCollection<VehicleSummaryResponse>>> GetAllActive()
    {
        var vehicles = await vehicleService.GetAllActiveAsync();
        return Ok(vehicles);
    }

    [HttpGet("my")]
    [Authorize(Policy = VehiclePolicies.Seller)]
    public async Task<ActionResult<IReadOnlyCollection<VehicleSummaryResponse>>> GetMy()
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        var vehicles = await vehicleService.GetByOwnerAsync(currentUser.UserId);
        return Ok(vehicles);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = VehiclePolicies.AuthenticatedUser)]
    public async Task<ActionResult<VehicleResponse>> GetById(Guid id)
    {
        try
        {
            var vehicle = await vehicleService.GetByIdAsync(id);
            return Ok(vehicle);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = VehiclePolicies.Seller)]
    public async Task<ActionResult<VehicleResponse>> Update(Guid id, UpdateVehicleRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            var response = await vehicleService.UpdateAsync(id, request, currentUser.UserId);
            return Ok(response);
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
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = VehiclePolicies.Seller)]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateVehicleStatusRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            await vehicleService.UpdateStatusAsync(id, request, currentUser.UserId);
            return NoContent();
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

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = VehiclePolicies.Seller)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { message = "User identity is missing or invalid." });
        }

        try
        {
            await vehicleService.DeleteAsync(id, currentUser.UserId);
            return NoContent();
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
}
