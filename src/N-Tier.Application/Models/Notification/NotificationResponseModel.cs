using System;

namespace N_Tier.Application.Models.Notification;

public class NotificationResponseModel
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string EventType { get; set; }

    public Guid? PaperId { get; set; }

    public Guid? TopicId { get; set; }

    public Guid? JournalId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
