using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IPaperRepository : IBaseRepository<Paper>
{
    Task<Paper> GetByIdAsync(Guid id);
    Task<IEnumerable<Paper>> GetPaperbyAuthorIdAsync(Guid authorId);
    Task<(IEnumerable<Paper> Results, int TotalCount)> GetPaginatedAsync(int page, int size);
}
