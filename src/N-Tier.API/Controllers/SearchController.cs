using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Search;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SearchController : ApiController
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Full-text search papers with Elasticsearch
    /// </summary>
    /// <param name="q">Search keyword</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10)</param>
    /// <param name="from">Start year</param>
    /// <param name="to">End year</param>
    /// <param name="language">Language filter</param>
    /// <param name="isOpenAccess">Open access filter</param>
    /// <returns>Search results with highlights</returns>
    [HttpGet("papers")]
    public async Task<IActionResult> SearchPapers(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] int? from = null,
        [FromQuery] int? to = null,
        [FromQuery] string language = null,
        [FromQuery] bool? isOpenAccess = null)
    {
        var request = new SearchPaperRequestModel
        {
            Q = q,
            Page = page,
            Size = size,
            From = from,
            To = to,
            Language = language,
            IsOpenAccess = isOpenAccess
        };

        var result = await _searchService.SearchPapersAsync(request);
        return Ok(ApiResult<SearchPaperResponseModel>.Success(result));
    }
}
