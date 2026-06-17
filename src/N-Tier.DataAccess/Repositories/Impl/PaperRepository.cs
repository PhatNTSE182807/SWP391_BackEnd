using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class PaperRepository : BaseRepository<Paper>, IPaperRepository
{
    public PaperRepository(DatabaseContext context) : base(context) { }

    public async Task<Paper> GetByIdAsync(Guid id)
    {
        var paper = await Context.Papers
            .AsNoTracking()
            .Include(p => p.Journal)
            .Include(p => p.PaperAuthors)
                .ThenInclude(pa => pa.Author)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
            throw new N_Tier.Core.Exceptions.ResourceNotFoundException(typeof(Paper));

        return paper;
    }

    public async Task<IEnumerable<Paper>> GetPaperbyAuthorIdAsync(Guid authorId)
    {
       var paper = await Context.Papers
            .AsNoTracking()
            .Include(j => j.Journal)
            .Include(p => p.PaperAuthors)
                .ThenInclude(a => a.Author)
                .Where(pa => pa.PaperAuthors.Any(pa => pa.AuthorId == authorId))
                .ToListAsync();     
        return paper;
    }

    public async Task<(IEnumerable<Paper> Results, int TotalCount)> GetPaginatedAsync(int page, int size)
    {
        var total = await Context.Papers.CountAsync();
        var results = await Context.Papers
            .AsNoTracking()
            .Include(p => p.Journal)
            .Include(p => p.PaperAuthors)
                .ThenInclude(pa => pa.Author)
            .OrderBy(p => p.Title)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (results, total);
    }
}
