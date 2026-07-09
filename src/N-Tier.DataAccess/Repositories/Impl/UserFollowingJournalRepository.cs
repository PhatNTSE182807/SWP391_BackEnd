using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class UserFollowingJournalRepository : BaseRepository<UserFollowingJournal>, IUserFollowingJournalRepository
{
    public UserFollowingJournalRepository(DatabaseContext context) : base(context) { }

    public async Task<List<UserFollowingJournal>> GetFollowingJournalsByUserIdAsync(Guid userId)
    {
        return await DbSet
            .Include(f => f.Journal)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserFollowingJournal> GetFollowAsync(Guid userId, Guid journalId)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.UserId == userId && f.JournalId == journalId);
    }

    public async Task<bool> IsFollowingAsync(Guid userId, Guid journalId)
    {
        return await DbSet.AnyAsync(f => f.UserId == userId && f.JournalId == journalId);
    }
}
