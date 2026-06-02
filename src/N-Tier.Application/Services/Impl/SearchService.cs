using Microsoft.Extensions.Logging;
using N_Tier.Application.Models.Search;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace N_Tier.Application.Services.Impl;

public class SearchService : ISearchService
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchService> _logger;
    private const string CacheKeyPrefix = "search:papers:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public SearchService(
        IElasticsearchService elasticsearchService,
        ICacheService cacheService,
        ILogger<SearchService> logger)
    {
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SearchPaperResponseModel> SearchPapersAsync(SearchPaperRequestModel request)
    {
        try
        {
            // Generate cache key from request parameters
            var cacheKey = GenerateCacheKey(request);

            // Try to get from cache
            var cachedResult = await _cacheService.GetAsync<SearchPaperResponseModel>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Cache hit for search query: {Query}", request.Q);
                return cachedResult;
            }

            _logger.LogInformation("Cache miss for search query: {Query}", request.Q);

            // Search in Elasticsearch
            var result = await _elasticsearchService.SearchPapersAsync(request);

            // Cache the result for 15 minutes
            if (result.Results.Any())
            {
                await _cacheService.SetAsync(cacheKey, result, CacheDuration);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching papers");
            return new SearchPaperResponseModel();
        }
    }

    private string GenerateCacheKey(SearchPaperRequestModel request)
    {
        // Serialize request to JSON and create hash for cache key
        var json = JsonSerializer.Serialize(request);
        var hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(json)));
        return $"{CacheKeyPrefix}{hash}";
    }
}
