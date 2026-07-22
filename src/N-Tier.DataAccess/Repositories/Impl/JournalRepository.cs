using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class JournalRepository : BaseRepository<Journal>, IJournalRepository
{
    public JournalRepository(DatabaseContext context) : base(context) { }

    public async Task<IEnumerable<Journal>> GetAllWithInclusionsAsync()
    {
        return await Context.Journals
            .AsNoTracking()
            .AsSplitQuery()
            .Include(j => j.JournalSourceMappings)
            .Include(j => j.JournalTopics)
                .ThenInclude(jt => jt.Topic)
            .Include(j => j.JournalTypeNavigation)
            .Include(j => j.Papers)
            .ToListAsync();
    }

    public async Task<Journal> GetByIdAsync(Guid id)
    {
        var journal = await Context.Journals
            .AsNoTracking()
            .AsSplitQuery()
            .Include(j => j.JournalSourceMappings)
            .Include(j => j.JournalTopics)
                .ThenInclude(jt => jt.Topic)
            .Include(j => j.JournalTypeNavigation)
            .Include(j => j.Papers)
            .FirstOrDefaultAsync(j => j.JournalId == id);

        if (journal == null)
            throw new N_Tier.Core.Exceptions.ResourceNotFoundException(typeof(Journal));

        return journal;
    }

    public async Task<(IEnumerable<Journal> Results, int TotalCount)> GetPaginatedAsync(int page, int size)
    {
        var total = await Context.Journals.CountAsync();
        var results = await Context.Journals
            .AsNoTracking()
            .AsSplitQuery()
            .Include(j => j.JournalSourceMappings)
            .Include(j => j.JournalTopics)
                .ThenInclude(jt => jt.Topic)
            .Include(j => j.JournalTypeNavigation)
            .Include(j => j.Papers)
            .OrderBy(j => j.JournalName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (results, total);
    }
}
