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
    /// Gets the profile information of the currently logged-in user.
    /// </summary>
    [HttpGet("profile")]
    [HttpGet("/api/profile")]
    public async Task<IActionResult> GetProfileAsync()
    {
        var result = await userService.GetProfileAsync();
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Allows the currently logged-in user to update their own personal information.
    /// If NewPassword is provided, OldPassword must also be provided and correct.
    /// </summary>
    [HttpPut("profile")]
    [HttpPut("/api/profile")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateUserProfileModel model)
    {
        var result = await userService.UpdateProfileAsync(model);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Gets the list of bookmarks for the current user.
    /// </summary>
    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarksAsync()
    {
        var result = await userService.GetBookmarksAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserBookmarkResponseModel>>.Success(result));
    }

    /// <summary>
    /// Bookmarks a paper.
    /// </summary>
    [HttpPost("bookmarks/{paperId}")]
    public async Task<IActionResult> AddBookmarkAsync([FromRoute] System.Guid paperId)
    {
        var result = await userService.AddBookmarkAsync(paperId);
        return Ok(ApiResult<UserBookmarkResponseModel>.Success(result));
    }

    /// <summary>
    /// Removes a bookmark from a paper.
    /// </summary>
    [HttpDelete("bookmarks/{paperId}")]
    public async Task<IActionResult> DeleteBookmarkAsync([FromRoute] System.Guid paperId)
    {
        await userService.DeleteBookmarkAsync(paperId);
        return Ok(ApiResult<string>.Success("Bookmark removed successfully"));
    }

    /// <summary>
    /// Gets the list of topics the current user is following.
    /// </summary>
    [HttpGet("following/topics")]
    public async Task<IActionResult> GetFollowingTopicsAsync()
    {
        var result = await userService.GetFollowingTopicsAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserFollowingTopicResponseModel>>.Success(result));
    }

    /// <summary>
    /// Follows a topic.
    /// </summary>
    [HttpPost("following/topics/{topicId}")]
    public async Task<IActionResult> FollowTopicAsync([FromRoute] System.Guid topicId)
    {
        var result = await userService.FollowTopicAsync(topicId);
        return Ok(ApiResult<UserFollowingTopicResponseModel>.Success(result));
    }

    /// <summary>
    /// Unfollows a topic.
    /// </summary>
    [HttpDelete("following/topics/{topicId}")]
    public async Task<IActionResult> UnfollowTopicAsync([FromRoute] System.Guid topicId)
    {
        await userService.UnfollowTopicAsync(topicId);
        return Ok(ApiResult<string>.Success("Unfollowed topic successfully"));
    }

    /// <summary>
    /// Gets the list of journals the current user is following.
    /// </summary>
    [HttpGet("following/journals")]
    public async Task<IActionResult> GetFollowingJournalsAsync()
    {
        var result = await userService.GetFollowingJournalsAsync();
        return Ok(ApiResult<System.Collections.Generic.List<UserFollowingJournalResponseModel>>.Success(result));
    }

    /// <summary>
    /// Follows a journal.
    /// </summary>
    [HttpPost("following/journals/{journalId}")]
    public async Task<IActionResult> FollowJournalAsync([FromRoute] System.Guid journalId)
    {
        var result = await userService.FollowJournalAsync(journalId);
        return Ok(ApiResult<UserFollowingJournalResponseModel>.Success(result));
    }

    /// <summary>
    /// Unfollows a journal.
    /// </summary>
    [HttpDelete("following/journals/{journalId}")]
    public async Task<IActionResult> UnfollowJournalAsync([FromRoute] System.Guid journalId)
    {
        await userService.UnfollowJournalAsync(journalId);
        return Ok(ApiResult<string>.Success("Unfollowed journal successfully"));
    }

    /// <summary>
    /// Updates the FCM Device Token for the currently logged-in user.
    /// </summary>
    [HttpPost("device-token")]
    public async Task<IActionResult> UpdateDeviceTokenAsync([FromBody] UpdateDeviceTokenModel model)
    {
        await userService.UpdateDeviceTokenAsync(model);
        return Ok(ApiResult<string>.Success("Device token updated successfully"));
    }
}
