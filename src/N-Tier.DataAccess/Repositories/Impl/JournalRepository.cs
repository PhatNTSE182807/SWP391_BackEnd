using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class JournalRepository : BaseRepository<Journal>, IJournalRepository
{
    public JournalRepository(DatabaseContext context) : base(context) { }
}
