using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IJournalRepository : IBaseRepository<Journal>
{
    Task<IEnumerable<Journal>> GetAllWithInclusionsAsync();
    Task<Journal> GetByIdAsync(Guid id);
}
