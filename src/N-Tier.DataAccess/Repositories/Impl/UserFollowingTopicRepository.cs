using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class UserFollowingTopicRepository : BaseRepository<UserFollowingTopic>, IUserFollowingTopicRepository
{
    public UserFollowingTopicRepository(DatabaseContext context) : base(context) { }

    public async Task<List<UserFollowingTopic>> GetFollowingTopicsByUserIdAsync(Guid userId)
    {
        return await DbSet
            .Include(f => f.Topic)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserFollowingTopic> GetFollowAsync(Guid userId, Guid topicId)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TopicId == topicId);
    }

    public async Task<bool> IsFollowingAsync(Guid userId, Guid topicId)
    {
        return await DbSet.AnyAsync(f => f.UserId == userId && f.TopicId == topicId);
    }
}
