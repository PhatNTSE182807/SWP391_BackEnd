using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class KeywordRepository : BaseRepository<Keyword>, IKeywordRepository
{
    public KeywordRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Keyword> Results, int TotalCount)> GetPaginatedKeywordsAsync(string searchTerm, int page, int size)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(k => 
                k.NormalizedName.Contains(term) || 
                k.KeywordName.ToLower().Contains(term));
        }

        var total = await query.CountAsync();

        var results = await query
            .OrderByDescending(k => k.WorksCount ?? 0)
            .ThenBy(k => k.KeywordName)
            .Skip((page - 1) * size)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        return (results, total);
    }
}
