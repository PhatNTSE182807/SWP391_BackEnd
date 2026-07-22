using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
{
    public AuthorRepository(DatabaseContext context) : base(context) { }

    public async Task<IEnumerable<Author>> GetAllWithInclusionsAsync()
    {
        return await Context.Authors
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.AuthorSourceMappings)
            .Include(a => a.PaperAuthors)
                .ThenInclude(pa => pa.Paper)
                    .ThenInclude(p => p.Journal)
            .ToListAsync();
    }

    public async Task<Author> GetByIdAsync(Guid id)
    {
        var author = await Context.Authors
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.AuthorSourceMappings)
            .Include(a => a.PaperAuthors)
                .ThenInclude(pa => pa.Paper)
                    .ThenInclude(p => p.Journal)
            .FirstOrDefaultAsync(a => a.AuthorId == id);

        if (author == null)
            throw new N_Tier.Core.Exceptions.ResourceNotFoundException(typeof(Author));

        return author;
    }

    public async Task<(IEnumerable<Author> Results, int TotalCount)> GetPaginatedAsync(int page, int size)
    {
        var total = await Context.Authors.CountAsync();
        var results = await Context.Authors
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.AuthorSourceMappings)
            .Include(a => a.PaperAuthors)
                .ThenInclude(pa => pa.Paper)
                    .ThenInclude(p => p.Journal)
            .OrderBy(a => a.DisplayName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (results, total);
    }
}
