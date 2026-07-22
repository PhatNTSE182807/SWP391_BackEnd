using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Notification;

namespace N_Tier.Application.Services;

public interface INotificationService
{
    Task SendNewPaperInFollowedTopicNotificationAsync(Guid paperId);
    Task SendNewPaperInFollowedJournalNotificationAsync(Guid paperId);
    Task SendBookmarkedPaperUpdatedNotificationAsync(Guid paperId);

    Task<List<NotificationResponseModel>> GetUserNotificationsAsync();
    Task<int> GetUnreadCountAsync();
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync();
}
