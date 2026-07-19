using System;
using System.Threading.Tasks;

namespace N_Tier.Application.Services;

public interface INotificationService
{
    Task SendNewPaperInFollowedTopicNotificationAsync(Guid paperId);
    Task SendNewPaperInFollowedJournalNotificationAsync(Guid paperId);
    Task SendBookmarkedPaperUpdatedNotificationAsync(Guid paperId);
}
