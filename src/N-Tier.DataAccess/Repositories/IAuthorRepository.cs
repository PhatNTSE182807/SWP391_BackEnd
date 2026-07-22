using System;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

using System.Collections.Generic;

namespace N_Tier.DataAccess.Repositories;

public interface IAuthorRepository : IBaseRepository<Author>
{
    Task<IEnumerable<Author>> GetAllWithInclusionsAsync();
    Task<Author> GetByIdAsync(Guid id);
    Task<(IEnumerable<Author> Results, int TotalCount)> GetPaginatedAsync(int page, int size);
}
