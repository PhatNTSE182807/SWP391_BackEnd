using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Analytics;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Require authorization for all statistics to align with other endpoints
public class AnalyticsController : ApiController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    #region Research Trends

    [HttpGet("trends/papers-by-year")]
    public async Task<IActionResult> GetPaperCountByYear()
    {
        var result = await _analyticsService.GetPaperCountByYearAsync();
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("trends/citations-by-year")]
    public async Task<IActionResult> GetCitationsByYear()
    {
        var result = await _analyticsService.GetCitationsByYearAsync();
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("trends/top-topics")]
    public async Task<IActionResult> GetTopTopics([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopTopicsAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("trends/top-domains")]
    public async Task<IActionResult> GetTopDomains([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopDomainsAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("trends/keywords-over-time")]
    public async Task<IActionResult> GetKeywordTrendOverTime([FromQuery] List<string> keywords)
    {
        var result = await _analyticsService.GetKeywordTrendOverTimeAsync(keywords);
        return Ok(ApiResult<List<SeriesDataDto>>.Success(result));
    }

    #endregion

    #region Author Statistics

    [HttpGet("authors/top-citations")]
    public async Task<IActionResult> GetTopAuthorsByCitations([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopAuthorsByCitationsAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("authors/top-hindex")]
    public async Task<IActionResult> GetTopAuthorsByHIndex([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopAuthorsByHIndexAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("authors/collaboration-network")]
    public async Task<IActionResult> GetAuthorCollaborationNetwork([FromQuery] int size = 50)
    {
        var result = await _analyticsService.GetAuthorCollaborationNetworkAsync(size);
        return Ok(ApiResult<NetworkGraphDto>.Success(result));
    }

    #endregion

    #region Journal Statistics

    [HttpGet("journals/top-paper-count")]
    public async Task<IActionResult> GetTopJournalsByPaperCount([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopJournalsByPaperCountAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("journals/top-citations")]
    public async Task<IActionResult> GetTopJournalsByCitations([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopJournalsByCitationsAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("journals/open-access-ratio")]
    public async Task<IActionResult> GetOpenAccessRatio()
    {
        var result = await _analyticsService.GetOpenAccessRatioAsync();
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    #endregion

    #region Keyword Statistics

    [HttpGet("keywords/word-cloud")]
    public async Task<IActionResult> GetKeywordCloud([FromQuery] int size = 50)
    {
        var result = await _analyticsService.GetKeywordCloudAsync(size);
        return Ok(ApiResult<List<ChartDataPoint>>.Success(result));
    }

    [HttpGet("keywords/top-by-year")]
    public async Task<IActionResult> GetTopKeywordsByYear([FromQuery] int size = 10)
    {
        var result = await _analyticsService.GetTopKeywordsByYearAsync(size);
        return Ok(ApiResult<List<SeriesDataDto>>.Success(result));
    }

    [HttpGet("keywords/co-occurrence")]
    public async Task<IActionResult> GetKeywordCoOccurrenceNetwork([FromQuery] int size = 50)
    {
        var result = await _analyticsService.GetKeywordCoOccurrenceNetworkAsync(size);
        return Ok(ApiResult<NetworkGraphDto>.Success(result));
    }

    #endregion
}
