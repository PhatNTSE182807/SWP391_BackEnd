using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class PaperRepository : BaseRepository<Paper>, IPaperRepository
{
    public PaperRepository(DatabaseContext context) : base(context) { }
}
