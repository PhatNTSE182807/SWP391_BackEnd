using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IPaperRepository : IBaseRepository<Paper>
{
    Task<Paper> GetByIdAsync(Guid id);
    Task<IEnumerable<Paper>> GetPaperbyAuthorIdAsync(Guid authorId);
}
