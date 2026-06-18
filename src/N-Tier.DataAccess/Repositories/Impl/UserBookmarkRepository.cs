using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class UserBookmarkRepository : BaseRepository<UserBookmark>, IUserBookmarkRepository
{
    public UserBookmarkRepository(DatabaseContext context) : base(context) { }

    public async Task<List<UserBookmark>> GetBookmarksByUserIdAsync(Guid userId)
    {
        return await DbSet
            .Include(b => b.Paper)
                .ThenInclude(p => p.Journal)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserBookmark> GetBookmarkAsync(Guid userId, Guid paperId)
    {
        return await DbSet
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PaperId == paperId);
    }

    public async Task<bool> IsBookmarkedAsync(Guid userId, Guid paperId)
    {
        return await DbSet.AnyAsync(b => b.UserId == userId && b.PaperId == paperId);
    }
}
