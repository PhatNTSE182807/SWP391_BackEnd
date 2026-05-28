using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Journal;

namespace N_Tier.Application.Services;

public interface IJournalService
{
    Task<List<JournalResponseModel>> GetAllAsync();
    Task<JournalResponseModel> GetByIdAsync(Guid id);
    Task<JournalResponseModel> CreateAsync(CreateJournalModel model);
    Task<JournalResponseModel> UpdateAsync(Guid id, UpdateJournalModel model);
    Task<bool> DeleteAsync(Guid id);
}
