using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[Authorize(Roles = "Admin")]
public abstract class AdminControllerBase : ControllerBase
{
}
