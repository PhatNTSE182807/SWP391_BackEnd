using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.ApiSource;

namespace N_Tier.Application.Services;

public interface IApiSourceService
{
    Task<List<ApiSourceResponseModel>> GetAllAsync();
    Task<ApiSourceResponseModel> GetByIdAsync(Guid id);
    Task<ApiSourceResponseModel> CreateAsync(CreateApiSourceModel model);
    Task<ApiSourceResponseModel> UpdateAsync(Guid id, UpdateApiSourceModel model);
    Task<bool> DeleteAsync(Guid id);
}
