using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Journal;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace N_Tier.Application.Services.Impl;

public class JournalService : IJournalService
{
    private readonly IJournalRepository _journalRepository;
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public JournalService(IJournalRepository journalRepository, IDistributedCache cache)
    {
        _journalRepository = journalRepository;
        _cache = cache;
    }

    public async Task<List<JournalResponseModel>> GetAllAsync()
    {
        var entities = await _journalRepository.GetAllWithInclusionsAsync();
        return entities.Adapt<List<JournalResponseModel>>();
    }

    public async Task<JournalResponseModel> GetByIdAsync(Guid id)
    {
        var entity = await _journalRepository.GetByIdAsync(id);
        return entity.Adapt<JournalResponseModel>();
    }

    public async Task<JournalResponseModel> CreateAsync(CreateJournalModel model)
    {
        var entity = model.Adapt<Journal>();
        var createdEntity = await _journalRepository.AddAsync(entity);
        await InvalidateCacheAsync("journals");
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
        await InvalidateCacheAsync("journals");
        return updatedEntity.Adapt<JournalResponseModel>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _journalRepository.GetFirstAsync(e => e.JournalId == id);
        await _journalRepository.DeleteAsync(entity);
        await InvalidateCacheAsync("journals");
        return true;
    }

    public async Task<PagedResponse<JournalResponseModel>> GetPaginatedJournalsAsync(PagedRequest request)
    {
        var version = await GetCacheVersionAsync("journals");
        var cacheKey = $"journals:v:{version}:page:{request.Page}:size:{request.Size}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<PagedResponse<JournalResponseModel>>(cachedData, _jsonOptions);
        }

        var (results, total) = await _journalRepository.GetPaginatedAsync(request.Page, request.Size);
        var mappedResults = results.Adapt<List<JournalResponseModel>>();
        var response = new PagedResponse<JournalResponseModel>(mappedResults, total, request.Page, request.Size);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response, _jsonOptions), cacheOptions);

        return response;
    }

    private async Task InvalidateCacheAsync(string prefix)
    {
        var versionStr = await _cache.GetStringAsync($"{prefix}:version");
        if (int.TryParse(versionStr, out var version))
        {
            await _cache.SetStringAsync($"{prefix}:version", (version + 1).ToString());
        }
        else
        {
            await _cache.SetStringAsync($"{prefix}:version", "2");
        }
    }

    private async Task<string> GetCacheVersionAsync(string prefix)
    {
        var version = await _cache.GetStringAsync($"{prefix}:version");
        if (string.IsNullOrEmpty(version))
        {
            version = "1";
            await _cache.SetStringAsync($"{prefix}:version", version);
        }
        return version;
    }
}
