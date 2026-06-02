using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Jobs;
using N_Tier.Application.Models;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Add role check in production: [Authorize(Roles = "Admin")]
public class AdminController : ApiController
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;

    public AdminController(
        IBackgroundJobClient backgroundJobs,
        IElasticsearchService elasticsearchService,
        ICacheService cacheService)
    {
        _backgroundJobs = backgroundJobs;
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Trigger manual reindexing of all papers
    /// </summary>
    [HttpPost("reindex")]
    public IActionResult TriggerReindex()
    {
        var jobId = _backgroundJobs.Enqueue<IReindexJob>(job => job.ExecuteAsync());
        return Ok(ApiResult<object>.Success(new { jobId, message = "Reindex job queued successfully" }));
    }

    /// <summary>
    /// Create Elasticsearch index
    /// </summary>
    [HttpPost("elasticsearch/create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        var result = await _elasticsearchService.CreateIndexAsync();
        return Ok(ApiResult<bool>.Success(result));
    }

    /// <summary>
    /// Delete Elasticsearch index
    /// </summary>
    [HttpDelete("elasticsearch/delete-index")]
    public async Task<IActionResult> DeleteIndex()
    {
        var result = await _elasticsearchService.DeleteIndexAsync();
        return Ok(ApiResult<bool>.Success(result));
    }

    /// <summary>
    /// Clear all search cache
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache()
    {
        var result = await _cacheService.RemoveByPatternAsync("search:papers:*");
        return Ok(ApiResult<object>.Success(new { cleared = result, message = "Cache cleared successfully" }));
    }
}
