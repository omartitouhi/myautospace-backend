using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        var config = await configService.GetByKeyAsync(key);

        return config is null
            ? NotFound(new { message = "Configuration key was not found." })
            : Ok(config);
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<SystemConfigResponse>> Upsert(string key, UpdateSystemConfigRequest request)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        var config = await configService.UpsertAsync(key, request, adminUserId.Value);
        return Ok(config);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
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
