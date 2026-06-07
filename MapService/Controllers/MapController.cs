using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MapService.Application.DTOs;
using MapService.Application.Interfaces;
using MapService.Domain.Constants;
using MapService.Domain.Enums;

namespace MapService.Controllers;

[ApiController]
[Route("api/maps")]
public class MapController(IMapLocationService mapLocationService, ICurrentUserService currentUserService)
    : ControllerBase
{
    // ── POST /api/maps/locations ─────────────────────────────────────────────

    [HttpPost("locations")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(MapLocationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await mapLocationService.CreateAsync(request, currentUser.UserId);
            return CreatedAtAction(nameof(GetLocationById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── GET /api/maps/locations/{id} ─────────────────────────────────────────

    [HttpGet("locations/{id:guid}")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(MapLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationById([FromRoute] Guid id)
    {
        try
        {
            var response = await mapLocationService.GetByIdAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── GET /api/maps/locations/entity/{entityId}?entityType=Vehicle ─────────

    [HttpGet("locations/entity/{entityId:guid}")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(MapLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationByEntity(
        [FromRoute] Guid entityId,
        [FromQuery] EntityType entityType)
    {
        try
        {
            var response = await mapLocationService.GetByEntityAsync(entityId, entityType);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── GET /api/maps/locations/my ───────────────────────────────────────────

    [HttpGet("locations/my")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(IReadOnlyCollection<MapLocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLocations()
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var response = await mapLocationService.GetByOwnerAsync(currentUser.UserId);
        return Ok(response);
    }

    // ── GET /api/maps/locations/nearby ───────────────────────────────────────

    [HttpGet("locations/nearby")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(IReadOnlyCollection<NearbyLocationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyLocations([FromQuery] NearbySearchRequest request)
    {
        try
        {
            var results = await mapLocationService.GetNearbyAsync(request);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── PUT /api/maps/locations/{id} ─────────────────────────────────────────

    [HttpPut("locations/{id:guid}")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(typeof(MapLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocation([FromRoute] Guid id, [FromBody] UpdateLocationRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await mapLocationService.UpdateAsync(id, request, currentUser.UserId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── DELETE /api/maps/locations/{id} ──────────────────────────────────────

    [HttpDelete("locations/{id:guid}")]
    [Authorize(Policy = MapPolicies.AuthenticatedUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLocation([FromRoute] Guid id)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            await mapLocationService.DeleteAsync(id, currentUser.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }
}
