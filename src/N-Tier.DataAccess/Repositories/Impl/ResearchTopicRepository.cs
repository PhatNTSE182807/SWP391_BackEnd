using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class ResearchTopicRepository : BaseRepository<ResearchTopic>, IResearchTopicRepository
{
    public ResearchTopicRepository(DatabaseContext context) : base(context) { }

    public async Task<(IEnumerable<ResearchTopic> Results, int TotalCount)> GetPaginatedAsync(int page, int size)
    {
        var total = await Context.ResearchTopics.CountAsync();
        var results = await Context.ResearchTopics
            .AsNoTracking()
            .OrderBy(t => t.TopicName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (results, total);
    }
}
