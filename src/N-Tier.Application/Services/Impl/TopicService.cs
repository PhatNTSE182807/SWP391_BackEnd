using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Topic;
using N_Tier.DataAccess.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace N_Tier.Application.Services.Impl
{
    public class TopicService : ITopicService
    {
        private readonly IResearchTopicRepository _researchTopicRepository;
        private readonly IDistributedCache _cache;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TopicService(IResearchTopicRepository researchTopicRepository, IDistributedCache cache)
        {
            _researchTopicRepository = researchTopicRepository;
            _cache = cache;
        }

        public async Task<List<TopicResponseModel>> GetAllTopicsAsync()
        {
            var topics = await _researchTopicRepository.GetAllAsync(t => true);
            return topics.Adapt<List<TopicResponseModel>>();
        }

        public async Task<TopicResponseModel> GetTopicByIdAsync(Guid id)
        {
            var topics = await _researchTopicRepository.GetAllAsync(t => t.TopicId == id);
            var topic = topics.FirstOrDefault();
            return topic?.Adapt<TopicResponseModel>();
        }

        public async Task<PagedResponse<TopicResponseModel>> GetPaginatedTopicsAsync(TopicPagedRequest request)
        {
            var version = await GetCacheVersionAsync("topics");
            var cacheKey = $"topics:v:{version}:page:{request.Page}:size:{request.Size}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<PagedResponse<TopicResponseModel>>(cachedData, _jsonOptions);
            }

            var (results, total) = await _researchTopicRepository.GetPaginatedAsync(request.Page, request.Size);
            var mappedResults = results.Adapt<List<TopicResponseModel>>();
            var response = new PagedResponse<TopicResponseModel>(mappedResults, total, request.Page, request.Size);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response, _jsonOptions), cacheOptions);

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
