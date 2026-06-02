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

    /// <summary>
    /// Lấy danh sách bookmarks của user.
    /// </summary>
    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarksAsync()
    {
        var result = await userService.GetBookmarksAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserBookmarkResponseModel>>.Success(result));
    }

    /// <summary>
    /// Bookmark một bài báo.
    /// </summary>
    [HttpPost("bookmarks/{paperId}")]
    public async Task<IActionResult> AddBookmarkAsync([FromRoute] System.Guid paperId)
    {
        var result = await userService.AddBookmarkAsync(paperId);
        return Ok(ApiResult<UserBookmarkResponseModel>.Success(result));
    }

    /// <summary>
    /// Xóa bookmark của một bài báo.
    /// </summary>
    [HttpDelete("bookmarks/{paperId}")]
    public async Task<IActionResult> DeleteBookmarkAsync([FromRoute] System.Guid paperId)
    {
        await userService.DeleteBookmarkAsync(paperId);
        return Ok(ApiResult<string>.Success("Bookmark removed successfully"));
    }

    /// <summary>
    /// Lấy danh sách topics user đang follow.
    /// </summary>
    [HttpGet("following/topics")]
    public async Task<IActionResult> GetFollowingTopicsAsync()
    {
        var result = await userService.GetFollowingTopicsAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserFollowingTopicResponseModel>>.Success(result));
    }

    /// <summary>
    /// Follow một topic.
    /// </summary>
    [HttpPost("following/topics/{topicId}")]
    public async Task<IActionResult> FollowTopicAsync([FromRoute] System.Guid topicId)
    {
        var result = await userService.FollowTopicAsync(topicId);
        return Ok(ApiResult<UserFollowingTopicResponseModel>.Success(result));
    }

    /// <summary>
    /// Unfollow một topic.
    /// </summary>
    [HttpDelete("following/topics/{topicId}")]
    public async Task<IActionResult> UnfollowTopicAsync([FromRoute] System.Guid topicId)
    {
        await userService.UnfollowTopicAsync(topicId);
        return Ok(ApiResult<string>.Success("Unfollowed topic successfully"));
    }
}
