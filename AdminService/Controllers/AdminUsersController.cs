using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController(
    IUserServiceClient userServiceClient,
    ICurrentAdminService currentAdminService) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await userServiceClient.GetUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userServiceClient.GetUserByIdAsync(id, cancellationToken);

            return user is null
                ? NotFound(new { message = "User was not found." })
                : Ok(user);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteUserActionAsync(
            id,
            cancellationToken,
            userServiceClient.SuspendUserAsync,
            "User suspended successfully.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteUserActionAsync(
            id,
            cancellationToken,
            userServiceClient.ActivateUserAsync,
            "User activated successfully.");
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken cancellationToken)
    {
        return await ExecuteUserActionAsync(
            id,
            cancellationToken,
            userServiceClient.BlockUserAsync,
            "User blocked successfully.");
    }

    private async Task<IActionResult> ExecuteUserActionAsync(
        Guid id,
        CancellationToken cancellationToken,
        Func<Guid, Guid, CancellationToken, Task> userAction,
        string successMessage)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        try
        {
            await userAction(id, adminUserId.Value, cancellationToken);
            return Ok(new { message = successMessage });
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    private ObjectResult NotImplemented(string message)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message,
            targetService = "UserService"
        });
    }
}
