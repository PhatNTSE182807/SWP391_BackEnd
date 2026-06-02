using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.User;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Tags("User Management")]
[Authorize]
public class UserController(IUserService userService) : ApiController
{
    /// <summary>
    /// Lấy thông tin cá nhân của user đang đăng nhập.
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileAsync()
    {
        var result = await userService.GetProfileAsync();
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Cho phép user đang đăng nhập tự cập nhật thông tin cá nhân.
    /// Nếu truyền NewPassword, bắt buộc phải truyền đúng OldPassword.
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateUserProfileModel model)
    {
        var result = await userService.UpdateProfileAsync(model);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }
}
