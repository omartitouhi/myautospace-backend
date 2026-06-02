using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/config")]
public class ConfigController(
    IConfigService configService,
    ICurrentAdminService currentAdminService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SystemConfigResponse>>> GetAll()
    {
        var configs = await configService.GetAllAsync();
        return Ok(configs);
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<SystemConfigResponse>> GetByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Configuration key is required." });
        }

        var config = await configService.GetByKeyAsync(key);

        return config is null
            ? NotFound(new { message = "Configuration key was not found." })
            : Ok(config);
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<SystemConfigResponse>> Upsert(string key, UpdateSystemConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Configuration key is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest(new { message = "Configuration value is required." });
        }

        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        try
        {
            var config = await configService.UpsertAsync(key, request, adminUserId.Value);
            return Ok(config);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_SystemConfigs_Key"
            })
        {
            return Conflict(new { message = "Configuration key already exists." });
        }
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Configuration key is required." });
        }

        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var deleted = await configService.DeleteAsync(key, adminUserId.Value);

        return deleted
            ? NoContent()
            : NotFound(new { message = "Configuration key was not found." });
    }
}
