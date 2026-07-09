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
    [HttpGet("/api/profile")]
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

    /// <summary>
    /// Lấy danh sách journals user đang follow.
    /// </summary>
    [HttpGet("following/journals")]
    public async Task<IActionResult> GetFollowingJournalsAsync()
    {
        var result = await userService.GetFollowingJournalsAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserFollowingJournalResponseModel>>.Success(result));
    }

    /// <summary>
    /// Follow một journal.
    /// </summary>
    [HttpPost("following/journals/{journalId}")]
    public async Task<IActionResult> FollowJournalAsync([FromRoute] System.Guid journalId)
    {
        var result = await userService.FollowJournalAsync(journalId);
        return Ok(ApiResult<UserFollowingJournalResponseModel>.Success(result));
    }

    /// <summary>
    /// Unfollow một journal.
    /// </summary>
    [HttpDelete("following/journals/{journalId}")]
    public async Task<IActionResult> UnfollowJournalAsync([FromRoute] System.Guid journalId)
    {
        await userService.UnfollowJournalAsync(journalId);
        return Ok(ApiResult<string>.Success("Unfollowed journal successfully"));
    }
}
