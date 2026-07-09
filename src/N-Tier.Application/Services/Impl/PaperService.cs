using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Paper;
using N_Tier.DataAccess.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace N_Tier.Application.Services.Impl
{
    public class PaperService : IPaperService
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IDistributedCache _cache;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public PaperService(IPaperRepository paperRepository, IDistributedCache cache)
        {
            _paperRepository = paperRepository;
            _cache = cache;
        }

        public async Task<List<PaperResponseModel>> GetAllPapersAsync()
        {
            var paper = await _paperRepository.GetAllAsync(p => true);
            return paper.Adapt<List<PaperResponseModel>>();
        }

        public async Task<PagedResponse<PaperResponseModel>> GetPaginatedPapersAsync(PagedRequest request)
        {
            var version = await GetCacheVersionAsync("papers");
            var cacheKey = $"papers:v:{version}:page:{request.Page}:size:{request.Size}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PagedResponse<PaperResponseModel>>(cachedData, _jsonOptions);
            }

            var (results, total) = await _paperRepository.GetPaginatedAsync(request.Page, request.Size);
            var mappedResults = results.Adapt<List<PaperResponseModel>>();
            var response = new PagedResponse<PaperResponseModel>(mappedResults, total, request.Page, request.Size);

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response, _jsonOptions), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return response;
        }

        public async Task<List<PaperResponseModel>> GetPaperbyAuthorId(Guid authorId)
        {
            var paper = await _paperRepository.GetPaperbyAuthorIdAsync(authorId);
            return paper.Adapt<List<PaperResponseModel>>();
        }

        public Task<PaperResponseModel> GetPaperByIdAsync(Guid id)
        {
            return GetPaperDetailAsync(id);
        }

        private async Task<PaperResponseModel> GetPaperDetailAsync(Guid id)
        {
            var cacheKey = $"paper:detail:{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PaperResponseModel>(cachedData, _jsonOptions);
            }

            var paper = await _paperRepository.GetByIdAsync(id);
            var response = paper.Adapt<PaperResponseModel>();

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response, _jsonOptions), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            return response;
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
}
