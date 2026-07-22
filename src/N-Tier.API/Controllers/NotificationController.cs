using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Tags("Firebase Notifications")]
[Authorize(Roles = "Admin,System Administrator")]
public class NotificationController(INotificationService notificationService) : ApiController
{
    /// <summary>
    /// Trigger "NewPaperInFollowedTopic" notification for a given paper ID.
    /// Finds paper topics, finds users following those topics, and sends FCM notifications.
    /// </summary>
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
    [HttpPost("trigger/bookmarked-paper-updated/{paperId:guid}")]
    public async Task<IActionResult> TriggerBookmarkedPaperUpdatedAsync([FromRoute] Guid paperId)
    {
        await notificationService.SendBookmarkedPaperUpdatedNotificationAsync(paperId);
        return Ok(ApiResult<string>.Success("BookmarkedPaperUpdated notification trigger completed"));
    }
}
