using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IUserFollowingJournalRepository : IBaseRepository<UserFollowingJournal>
{
    Task<List<UserFollowingJournal>> GetFollowingJournalsByUserIdAsync(Guid userId);
    Task<UserFollowingJournal> GetFollowAsync(Guid userId, Guid journalId);
    Task<bool> IsFollowingAsync(Guid userId, Guid journalId);
}
