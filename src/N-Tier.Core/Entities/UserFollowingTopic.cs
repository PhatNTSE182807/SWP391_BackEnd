using System;

namespace N_Tier.Core.Entities;

public class UserFollowingTopic
{
    public Guid FollowId { get; set; }

    public Guid UserId { get; set; }

    public Guid TopicId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }

    public virtual ResearchTopic Topic { get; set; }
}
