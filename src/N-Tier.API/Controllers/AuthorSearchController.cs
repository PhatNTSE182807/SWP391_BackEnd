using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Search;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/search/authors")]
[ApiController]
public class AuthorSearchController : ApiController
{
    private readonly ISearchService _searchService;
    private readonly IHangfireJobService _hangfireJobService;

    public AuthorSearchController(ISearchService searchService, IHangfireJobService hangfireJobService)
    {
        _searchService = searchService;
        _hangfireJobService = hangfireJobService;
    }

    /// <summary>
    /// Full-text search for authors using Elasticsearch
    /// Available to all authenticated users (User, Admin)
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Paginated search results with highlights</returns>
    [HttpGet]
    [Authorize] // All authenticated users can search
    public async Task<IActionResult> SearchAuthors([FromQuery] SearchAuthorRequest request)
    {
        var result = await _searchService.SearchAuthorsAsync(request);
        return Ok(ApiResult<SearchAuthorResponse>.Success(result));
    }

    /// <summary>
    /// Trigger bulk indexing of all authors to Elasticsearch (Admin only)
    /// </summary>
    [HttpPost("reindex")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReindexAuthors()
    {
        await _searchService.BulkIndexAuthorsAsync();
        return Ok(ApiResult<string>.Success("Author reindexing completed successfully"));
    }

    /// <summary>
    /// Enqueue a background job to reindex all authors (Admin only)
    /// </summary>
    [HttpPost("reindex/background")]
    [Authorize(Roles = "Admin")]
    public IActionResult ReindexAuthorsBackground()
    {
        var jobId = _hangfireJobService.EnqueueReindexAuthorsJob();
        return Ok(ApiResult<object>.Success(new { jobId, message = "Author reindexing job enqueued successfully" }));
    }

    /// <summary>
    /// Delete Elasticsearch index for authors (Admin only - DESTRUCTIVE!)
    /// </summary>
    [HttpDelete("index")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAuthorIndex()
    {
        await _searchService.DeleteAuthorIndexAsync();
        return Ok(ApiResult<string>.Success("Author index deleted successfully"));
    }

    /// <summary>
    /// Recreate Elasticsearch index with correct mapping and reindex all authors (Admin only)
    /// </summary>
    [HttpPost("index/recreate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecreateAuthorIndex()
    {
        await _searchService.RecreateAuthorIndexAsync();
        return Ok(ApiResult<string>.Success("Author index recreated and data reindexed successfully"));
    }

    /// <summary>
    /// Recreate author index in background (Admin only)
    /// </summary>
    [HttpPost("index/recreate/background")]
    [Authorize(Roles = "Admin")]
    public IActionResult RecreateAuthorIndexBackground()
    {
        var jobId = _hangfireJobService.EnqueueRecreateAuthorsIndexJob();
        return Ok(ApiResult<object>.Success(new { jobId, message = "Recreate author index job enqueued successfully" }));
    }
}
