using System;

namespace N_Tier.Core.Entities;

public class UserFollowingJournal
{
    public Guid FollowId { get; set; }

    public Guid UserId { get; set; }

    public Guid JournalId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }

    public virtual Journal Journal { get; set; }
}
