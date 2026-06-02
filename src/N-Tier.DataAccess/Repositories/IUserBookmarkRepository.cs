using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IUserBookmarkRepository : IBaseRepository<UserBookmark>
{
    Task<List<UserBookmark>> GetBookmarksByUserIdAsync(Guid userId);
    Task<UserBookmark> GetBookmarkAsync(Guid userId, Guid paperId);
    Task<bool> IsBookmarkedAsync(Guid userId, Guid paperId);
}
