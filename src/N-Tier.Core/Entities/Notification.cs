using System;

namespace N_Tier.Core.Entities;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string EventType { get; set; }

    public Guid? PaperId { get; set; }

    public Guid? TopicId { get; set; }

    public Guid? JournalId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

    public virtual User User { get; set; }

    public virtual Paper Paper { get; set; }
}
