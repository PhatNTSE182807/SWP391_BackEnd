using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IUserFollowingTopicRepository : IBaseRepository<UserFollowingTopic>
{
    Task<List<UserFollowingTopic>> GetFollowingTopicsByUserIdAsync(Guid userId);
    Task<UserFollowingTopic> GetFollowAsync(Guid userId, Guid topicId);
    Task<bool> IsFollowingAsync(Guid userId, Guid topicId);
}
