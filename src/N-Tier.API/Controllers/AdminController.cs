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
    /// Lấy danh sách tất cả users (username, email, phone, role, isActive).
    /// Chỉ dành cho System Administrator.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(ApiResult<object>.Success(users));
    }

    /// <summary>
    /// Toggle trạng thái active/deactivated của user.
    /// Admin không thể deactivate chính mình.
    /// Chỉ dành cho System Administrator.
    /// </summary>
    [HttpPut("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> ToggleDeactivateUserAsync(Guid userId)
    {
        var result = await userService.ToggleDeactivateUserAsync(userId);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Xóa tài khoản user theo userId.
    /// Admin không thể tự xóa chính mình.
    /// Chỉ dành cho System Administrator.
    /// </summary>
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUserAsync(Guid userId)
    {
        await userService.DeleteUserAsync(userId);
        return Ok(ApiResult<object>.Success(new { message = "User deleted successfully" }));
    }
}
