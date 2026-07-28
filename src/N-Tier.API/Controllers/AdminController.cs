using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.User;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Tags("AdminUserManagement")]
[Authorize(Roles = "System Administrator")]
public class AdminController(IUserService userService) : ApiController
{
    /// <summary>
    /// Gets a paginated list of all users (username, email, phone, role, isActive).
    /// Only accessible by System Administrator.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersAsync([FromQuery] PagedRequest request)
    {
        var users = await userService.GetPaginatedUsersAsync(request);
        return Ok(ApiResult<PagedResponse<UserResponseModel>>.Success(users));
    }
   
    /// <summary>
    /// Toggles the active/deactivated status of a user.
    /// Admin cannot deactivate themselves.
    /// Only accessible by System Administrator.
    /// </summary>
    [HttpPut("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUserAsync(Guid userId)
    {
        var result = await userService.DeactivateUserAsync(userId);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Re-activates a previously deactivated user account.
    /// Only accessible by System Administrator.
    /// </summary>
    [HttpPut("users/{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUserAsync(Guid userId)
    {
        var result = await userService.ActivateUserAsync(userId);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Deletes a user account by userId.
    /// Admin cannot delete their own account.
    /// Only accessible by System Administrator.
    /// </summary>
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid userId)
    {
        await userService.DeleteUserAsync(userId);
        return Ok(ApiResult<object>.Success(new { message = "User deleted successfully" }));
    }
}
