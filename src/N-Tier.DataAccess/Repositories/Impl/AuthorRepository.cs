using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
{
    public AuthorRepository(DatabaseContext context) : base(context) { }

    public async Task<Author> GetByIdAsync(Guid id)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.AuthorId == id);
    }
}
