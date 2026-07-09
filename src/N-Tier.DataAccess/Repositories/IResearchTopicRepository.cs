using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface IResearchTopicRepository : IBaseRepository<ResearchTopic>
{
    Task<(IEnumerable<ResearchTopic> Results, int TotalCount)> GetPaginatedAsync(int page, int size);
}
