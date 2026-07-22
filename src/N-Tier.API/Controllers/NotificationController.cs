using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Notification;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Tags("Notifications")]
[Authorize]
public class NotificationController(INotificationService notificationService) : ApiController
{
    /// <summary>
    /// Lấy danh sách tất cả thông báo của người dùng đang đăng nhập (sắp xếp mới nhất xếp trước).
    /// </summary>
    [HttpGet]
    [HttpGet("/api/notifications")]
    public async Task<IActionResult> GetNotificationsAsync()
    {
        var result = await notificationService.GetUserNotificationsAsync();
        return Ok(ApiResult<List<NotificationResponseModel>>.Success(result));
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc của người dùng đang đăng nhập.
    /// </summary>
    [HttpGet("unread-count")]
    [HttpGet("/api/notifications/unread-count")]
    public async Task<IActionResult> GetUnreadCountAsync()
    {
        var count = await notificationService.GetUnreadCountAsync();
        return Ok(ApiResult<int>.Success(count));
    }

    /// <summary>
    /// Đánh dấu một thông báo cụ thể là đã đọc.
    /// </summary>
    [HttpPut("{id:guid}/read")]
    [HttpPut("/api/notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkAsReadAsync([FromRoute] Guid id)
    {
        await notificationService.MarkAsReadAsync(id);
        return Ok(ApiResult<string>.Success("Notification marked as read"));
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo của người dùng đang đăng nhập là đã đọc.
    /// </summary>
    [HttpPut("read-all")]
    [HttpPut("/api/notifications/read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync()
    {
        await notificationService.MarkAllAsReadAsync();
        return Ok(ApiResult<string>.Success("All notifications marked as read"));
    }

    /// <summary>
    /// Trigger "NewPaperInFollowedTopic" notification for a given paper ID.
    /// Finds paper topics, finds users following those topics, and sends FCM notifications.
    /// </summary>
    [Authorize(Roles = "Admin,System Administrator")]
    [HttpPost("trigger/new-paper-topic/{paperId:guid}")]
    public async Task<IActionResult> TriggerNewPaperInFollowedTopicAsync([FromRoute] Guid paperId)
    {
        await notificationService.SendNewPaperInFollowedTopicNotificationAsync(paperId);
        return Ok(ApiResult<string>.Success("NewPaperInFollowedTopic notification trigger completed"));
    }

    /// <summary>
    /// Trigger "NewPaperInFollowedJournal" notification for a given paper ID.
    /// Finds paper journal, finds users following that journal, and sends FCM notifications.
    /// </summary>
    [Authorize(Roles = "Admin,System Administrator")]
    [HttpPost("trigger/new-paper-journal/{paperId:guid}")]
    public async Task<IActionResult> TriggerNewPaperInFollowedJournalAsync([FromRoute] Guid paperId)
    {
        await notificationService.SendNewPaperInFollowedJournalNotificationAsync(paperId);
        return Ok(ApiResult<string>.Success("NewPaperInFollowedJournal notification trigger completed"));
    }

    /// <summary>
    /// Trigger "BookmarkedPaperUpdated" notification for a given paper ID.
    /// Finds users who have bookmarked this paper and sends FCM notifications.
    /// </summary>
    [Authorize(Roles = "Admin,System Administrator")]
    [HttpPost("trigger/bookmarked-paper-updated/{paperId:guid}")]
    public async Task<IActionResult> TriggerBookmarkedPaperUpdatedAsync([FromRoute] Guid paperId)
    {
        await notificationService.SendBookmarkedPaperUpdatedNotificationAsync(paperId);
        return Ok(ApiResult<string>.Success("BookmarkedPaperUpdated notification trigger completed"));
    }
}
