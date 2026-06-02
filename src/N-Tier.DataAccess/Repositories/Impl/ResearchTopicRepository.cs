using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class ResearchTopicRepository : BaseRepository<ResearchTopic>, IResearchTopicRepository
{
    public ResearchTopicRepository(DatabaseContext context) : base(context) { }
}
