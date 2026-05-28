using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class KeywordRepository : BaseRepository<Keyword>, IKeywordRepository
{
    public KeywordRepository(DatabaseContext context) : base(context) { }
}
