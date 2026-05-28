using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.ApiSource;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Repositories;

namespace N_Tier.Application.Services.Impl;

public class ApiSourceService : IApiSourceService
{
    private readonly IApiSourceRepository _apiSourceRepository;

    public ApiSourceService(IApiSourceRepository apiSourceRepository)
    {
        _apiSourceRepository = apiSourceRepository;
    }

    public async Task<List<ApiSourceResponseModel>> GetAllAsync()
    {
        var entities = await _apiSourceRepository.GetAllAsync(_ => true);
        return entities.Adapt<List<ApiSourceResponseModel>>();
    }

    public async Task<ApiSourceResponseModel> GetByIdAsync(Guid id)
    {
        var entity = await _apiSourceRepository.GetFirstAsync(e => e.SourceId == id);
        return entity.Adapt<ApiSourceResponseModel>();
    }

    public async Task<ApiSourceResponseModel> CreateAsync(CreateApiSourceModel model)
    {
        var entity = model.Adapt<ApiSource>();
        var createdEntity = await _apiSourceRepository.AddAsync(entity);
        return createdEntity.Adapt<ApiSourceResponseModel>();
    }

    public async Task<ApiSourceResponseModel> UpdateAsync(Guid id, UpdateApiSourceModel model)
    {
        var entity = await _apiSourceRepository.GetFirstAsync(e => e.SourceId == id);
        
        entity.SourceName = model.SourceName;
        entity.BaseUrl = model.BaseUrl;
        entity.IsActive = model.IsActive;

        var updatedEntity = await _apiSourceRepository.UpdateAsync(entity);
        return updatedEntity.Adapt<ApiSourceResponseModel>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _apiSourceRepository.GetFirstAsync(e => e.SourceId == id);
        await _apiSourceRepository.DeleteAsync(entity);
        return true;
    }
}
