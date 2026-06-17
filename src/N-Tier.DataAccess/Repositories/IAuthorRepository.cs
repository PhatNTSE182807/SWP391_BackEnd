using System;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IAuthorRepository : IBaseRepository<Author>
{
    Task<Author> GetByIdAsync(Guid id);
}
