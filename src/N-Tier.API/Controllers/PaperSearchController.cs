using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Search;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/search/papers")]
[ApiController]
public class PaperSearchController : ApiController
{
    private readonly ISearchService _searchService;
    private readonly IHangfireJobService _hangfireJobService;

    public PaperSearchController(ISearchService searchService, IHangfireJobService hangfireJobService)
    {
        _searchService = searchService;
        _hangfireJobService = hangfireJobService;
    }

    /// <summary>
    /// Full-text search for papers using Elasticsearch
    /// Available to all authenticated users (User, Admin)
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Paginated search results with highlights</returns>
    [HttpGet]
    [Authorize] // All authenticated users can search
    public async Task<IActionResult> SearchPapers([FromQuery] SearchPaperRequest request)
    {
        var result = await _searchService.SearchPapersAsync(request);
        return Ok(ApiResult<SearchPaperResponse>.Success(result));
    }

    /// <summary>
    /// Trigger bulk indexing of all papers to Elasticsearch (Admin only)
    /// This runs synchronously and may take time
    /// </summary>
    [HttpPost("reindex")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReindexPapers()
    {
        await _searchService.BulkIndexPapersAsync();
        return Ok(ApiResult<string>.Success("Reindexing completed successfully"));
    }

    /// <summary>
    /// Enqueue a background job to reindex all papers (Admin only)
    /// This uses Hangfire and returns immediately
    /// </summary>
    [HttpPost("reindex/background")]
    [Authorize(Roles = "Admin")]
    public IActionResult ReindexPapersBackground()
    {
        var jobId = _hangfireJobService.EnqueueReindexJob();
        return Ok(ApiResult<object>.Success(new { jobId, message = "Reindexing job enqueued successfully" }));
    }

    /// <summary>
    /// Delete Elasticsearch index (Admin only - DESTRUCTIVE!)
    /// </summary>
    [HttpDelete("index")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteIndex()
    {
        await _searchService.DeleteIndexAsync();
        return Ok(ApiResult<string>.Success("Index deleted successfully"));
    }

    /// <summary>
    /// Recreate Elasticsearch index with correct mapping and reindex all data (Admin only)
    /// This will DELETE the old index and create a new one
    /// </summary>
    [HttpPost("index/recreate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecreateIndex()
    {
        await _searchService.RecreateIndexAsync();
        return Ok(ApiResult<string>.Success("Index recreated and data reindexed successfully"));
    }

    /// <summary>
    /// Recreate index in background (Admin only - recommended for large datasets)
    /// </summary>
    [HttpPost("index/recreate/background")]
    [Authorize(Roles = "Admin")]
    public IActionResult RecreateIndexBackground()
    {
        var jobId = _hangfireJobService.EnqueueRecreateIndexJob();
        return Ok(ApiResult<object>.Success(new { jobId, message = "Recreate index job enqueued successfully" }));
    }
}
