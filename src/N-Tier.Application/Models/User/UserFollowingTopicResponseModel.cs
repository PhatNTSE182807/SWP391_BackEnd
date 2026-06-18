using System;

namespace N_Tier.Application.Models.User;

public class UserFollowingTopicResponseModel
{
    public Guid FollowId { get; set; }
    public Guid UserId { get; set; }
    public Guid TopicId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string TopicName { get; set; }
    public string NormalizedName { get; set; }
}
