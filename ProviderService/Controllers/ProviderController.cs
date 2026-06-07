using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProviderService.Application.DTOs;
using ProviderService.Application.Interfaces;
using ProviderService.Domain.Constants;

namespace ProviderService.Controllers;

[ApiController]
[Authorize]
[Route("api/providers")]
public class ProviderController(
    IProviderProfileService profileService,
    IServiceOfferingService offeringService,
    IProviderAvailabilityService availabilityService,
    IProviderGalleryService galleryService,
    ICurrentUserService currentUserService) : ControllerBase
{
    // ── Profiles ─────────────────────────────────────────────────────────────

    /// <summary>Create the provider profile for the authenticated ServiceProvider.</summary>
    [HttpPost]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ProviderProfileResponse>> CreateProfile(CreateProviderProfileRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await profileService.CreateAsync(request, currentUser.UserId);
            return CreatedAtAction(nameof(GetProfileById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get all active provider profiles (any authenticated user).</summary>
    [HttpGet]
    [Authorize(Policy = ProviderPolicies.AuthenticatedUser)]
    public async Task<ActionResult<IReadOnlyCollection<ProviderProfileSummaryResponse>>> GetAllActive()
    {
        var profiles = await profileService.GetAllActiveAsync();
        return Ok(profiles);
    }

    /// <summary>Get a specific provider profile by id (any authenticated user).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = ProviderPolicies.AuthenticatedUser)]
    public async Task<ActionResult<ProviderProfileResponse>> GetProfileById(Guid id)
    {
        try
        {
            var profile = await profileService.GetByIdAsync(id);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Get the authenticated ServiceProvider's own profile.</summary>
    [HttpGet("my")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ProviderProfileResponse>> GetMyProfile()
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var profile = await profileService.GetByAuthUserIdAsync(currentUser.UserId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Update the authenticated ServiceProvider's own profile.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ProviderProfileResponse>> UpdateProfile(Guid id, UpdateProviderProfileRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await profileService.UpdateAsync(id, request, currentUser.UserId);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Soft-delete the authenticated ServiceProvider's own profile.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            await profileService.DeleteAsync(id, currentUser.UserId);
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

    // ── Service Offerings ────────────────────────────────────────────────────

    /// <summary>Add a service offering to a provider profile.</summary>
    [HttpPost("{profileId:guid}/services")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ServiceOfferingResponse>> AddOffering(Guid profileId, CreateServiceOfferingRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await offeringService.AddAsync(profileId, request, currentUser.UserId);
            return CreatedAtAction(nameof(GetProfileById), new { id = profileId }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update a service offering.</summary>
    [HttpPut("{profileId:guid}/services/{serviceId:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ServiceOfferingResponse>> UpdateOffering(Guid profileId, Guid serviceId, UpdateServiceOfferingRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await offeringService.UpdateAsync(profileId, serviceId, request, currentUser.UserId);
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
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a service offering.</summary>
    [HttpDelete("{profileId:guid}/services/{serviceId:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<IActionResult> DeleteOffering(Guid profileId, Guid serviceId)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            await offeringService.DeleteAsync(profileId, serviceId, currentUser.UserId);
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

    // ── Availability ─────────────────────────────────────────────────────────

    /// <summary>Get availability slots for a provider profile (any authenticated user).</summary>
    [HttpGet("{profileId:guid}/availability")]
    [Authorize(Policy = ProviderPolicies.AuthenticatedUser)]
    public async Task<ActionResult<IReadOnlyCollection<ProviderAvailabilityResponse>>> GetAvailability(Guid profileId)
    {
        var slots = await availabilityService.GetByProfileAsync(profileId);
        return Ok(slots);
    }

    /// <summary>Set (upsert) an availability slot for a day of the week.</summary>
    [HttpPut("{profileId:guid}/availability")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ProviderAvailabilityResponse>> SetAvailability(Guid profileId, SetAvailabilityRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await availabilityService.SetAsync(profileId, request, currentUser.UserId);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete an availability slot.</summary>
    [HttpDelete("{profileId:guid}/availability/{availabilityId:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<IActionResult> DeleteAvailability(Guid profileId, Guid availabilityId)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            await availabilityService.DeleteAsync(profileId, availabilityId, currentUser.UserId);
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

    // ── Gallery ───────────────────────────────────────────────────────────────

    /// <summary>Add a gallery image to a provider profile.</summary>
    [HttpPost("{profileId:guid}/gallery")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<ActionResult<ProviderGalleryResponse>> AddGalleryImage(Guid profileId, AddGalleryImageRequest request)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            var response = await galleryService.AddImageAsync(profileId, request, currentUser.UserId);
            return CreatedAtAction(nameof(GetProfileById), new { id = profileId }, response);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a gallery image.</summary>
    [HttpDelete("{profileId:guid}/gallery/{imageId:guid}")]
    [Authorize(Policy = ProviderPolicies.ServiceProvider)]
    public async Task<IActionResult> DeleteGalleryImage(Guid profileId, Guid imageId)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized(new { message = "User identity is missing or invalid." });

        try
        {
            await galleryService.DeleteImageAsync(profileId, imageId, currentUser.UserId);
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
