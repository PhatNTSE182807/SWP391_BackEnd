using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Journal;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Repositories;

namespace N_Tier.Application.Services.Impl;

public class JournalService : IJournalService
{
    private readonly IJournalRepository _journalRepository;

    public JournalService(IJournalRepository journalRepository)
    {
        _journalRepository = journalRepository;
    }

    public async Task<List<JournalResponseModel>> GetAllAsync()
    {
        var entities = await _journalRepository.GetAllAsync(_ => true);
        return entities.Adapt<List<JournalResponseModel>>();
    }

    public async Task<JournalResponseModel> GetByIdAsync(Guid id)
    {
        var entity = await _journalRepository.GetFirstAsync(e => e.JournalId == id);
        return entity.Adapt<JournalResponseModel>();
    }

    public async Task<JournalResponseModel> CreateAsync(CreateJournalModel model)
    {
        var entity = model.Adapt<Journal>();
        var createdEntity = await _journalRepository.AddAsync(entity);
        return createdEntity.Adapt<JournalResponseModel>();
    }

    public async Task<JournalResponseModel> UpdateAsync(Guid id, UpdateJournalModel model)
    {
        var entity = await _journalRepository.GetFirstAsync(e => e.JournalId == id);
        
        entity.JournalName = model.JournalName;
        entity.IssnL = model.IssnL;
        entity.Publisher = model.Publisher;
        entity.HomepageUrl = model.HomepageUrl;
        entity.IsOpenAccess = model.IsOpenAccess;
        entity.IsCore = model.IsCore;
        entity.UpdatedAt = DateTime.UtcNow;

        var updatedEntity = await _journalRepository.UpdateAsync(entity);
        return updatedEntity.Adapt<JournalResponseModel>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _journalRepository.GetFirstAsync(e => e.JournalId == id);
        await _journalRepository.DeleteAsync(entity);
        return true;
    }
}
