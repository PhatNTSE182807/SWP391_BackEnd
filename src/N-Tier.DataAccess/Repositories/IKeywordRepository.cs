using N_Tier.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace N_Tier.DataAccess.Repositories;

public interface IKeywordRepository : IBaseRepository<Keyword> 
{
    Task<(IEnumerable<Keyword> Results, int TotalCount)> GetPaginatedKeywordsAsync(string searchTerm, int page, int size);
}
