using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Analytics;
using N_Tier.Application.Services;
using N_Tier.Shared.Services;

namespace N_Tier.API.Controllers;

[Tags("Analytics")]
[Route("api/[controller]")]
[ApiController]
[Authorize] // Require authorization for all statistics to align with other endpoints and secure user access
public class AnalyticsController : ApiController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IClaimService _claimService;

    public AnalyticsController(IAnalyticsService analyticsService, IClaimService claimService)
    {
        _analyticsService = analyticsService;
        _claimService = claimService;
    }

    #region Research Trends (Elasticsearch)

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

    #region Author Statistics (Elasticsearch)

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

    #region Journal Statistics (Elasticsearch)

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

    [HttpGet("journals/tracker")]
    public async Task<IActionResult> GetJournalTracker([FromQuery] int size = 20, [FromQuery] int years = 5)
    {
        if (size <= 0 || size > 100)
            return BadRequest(ApiResult<List<JournalTrackerDto>>.Failure(new[] { "size must be between 1 and 100." }));

        if (years <= 0 || years > 20)
            return BadRequest(ApiResult<List<JournalTrackerDto>>.Failure(new[] { "years must be between 1 and 20." }));

        var result = await _analyticsService.GetJournalTrackerAsync(size, years);
        return Ok(ApiResult<List<JournalTrackerDto>>.Success(result));
    }

    #endregion

    #region Keyword Statistics (Elasticsearch)

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

    #region Topic Network (EF Core)

    [HttpGet("topics/co-occurrence")]
    public async Task<IActionResult> GetTopicCoOccurrenceNetwork([FromQuery] int size = 50)
    {
        if (size <= 0 || size > 200)
            return BadRequest(ApiResult<NetworkGraphDto>.Failure(new[] { "size must be between 1 and 200." }));

        var result = await _analyticsService.GetTopicCoOccurrenceNetworkAsync(size);
        return Ok(ApiResult<NetworkGraphDto>.Success(result));
    }

    #endregion

    #region Keyword & Topic Trends (EF Core)

    /// <summary>
    /// Returns yearly paper counts for a keyword to draw a line chart.
    /// </summary>
    /// <param name="keyword">The keyword to search (e.g., "machine learning")</param>
    /// <param name="years">Number of past years to include (default: 5)</param>
    [HttpGet("keyword-trends")]
    public async Task<IActionResult> GetKeywordTrendsAsync([FromQuery] string keyword, [FromQuery] int years = 5)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(ApiResult<KeywordTrendDto>.Failure(new[] { "keyword is required." }));

        var result = await _analyticsService.GetKeywordTrendsAsync(keyword, years);
        return Ok(ApiResult<KeywordTrendDto>.Success(result));
    }

    /// <summary>
    /// Returns yearly paper counts for a research topic to draw a line chart.
    /// </summary>
    /// <param name="topic">The topic name to search (e.g., "Machine Learning in Healthcare")</param>
    /// <param name="years">Number of past years to include (default: 5)</param>
    [HttpGet("topic-trends")]
    public async Task<IActionResult> GetTopicTrendsAsync([FromQuery] string topic, [FromQuery] int years = 5)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return BadRequest(ApiResult<TopicTrendDto>.Failure(new[] { "topic is required." }));

        var result = await _analyticsService.GetTopicTrendsAsync(topic, years);
        return Ok(ApiResult<TopicTrendDto>.Success(result));
    }

    /// <summary>
    /// Returns only topics that have linked papers, intended for topic comparison dropdowns.
    /// </summary>
    [HttpGet("topics/available-for-compare")]
    public async Task<IActionResult> GetAvailableTopicsForCompareAsync([FromQuery] string q = "", [FromQuery] int size = 300)
    {
        if (size <= 0 || size > 1000)
            return BadRequest(ApiResult<List<AvailableTopicForCompareDto>>.Failure(new[] { "size must be between 1 and 1000." }));

        var result = await _analyticsService.GetAvailableTopicsForCompareAsync(q, size);
        return Ok(ApiResult<List<AvailableTopicForCompareDto>>.Success(result));
    }

    /// <summary>
    /// Compares selected research topics by publications, citations, journal coverage, h-index, and yearly counts.
    /// </summary>
    [HttpGet("topics/compare")]
    public async Task<IActionResult> CompareTopicsAsync([FromQuery] List<Guid> topicIds, [FromQuery] int years = 5)
    {
        if (topicIds == null || topicIds.Count < 2)
            return BadRequest(ApiResult<List<TopicComparisonDto>>.Failure(new[] { "At least 2 topicIds are required." }));

        if (topicIds.Count > 5)
            return BadRequest(ApiResult<List<TopicComparisonDto>>.Failure(new[] { "A maximum of 5 topicIds can be compared." }));

        if (years <= 0 || years > 20)
            return BadRequest(ApiResult<List<TopicComparisonDto>>.Failure(new[] { "years must be between 1 and 20." }));

        var result = await _analyticsService.CompareTopicsAsync(topicIds.Distinct().ToList(), years);
        return Ok(ApiResult<List<TopicComparisonDto>>.Success(result));
    }

    /// <summary>
    /// Returns topics with highest publication growth over the latest available 1, 5, or 10 year window.
    /// </summary>
    [HttpGet("trending-topics")]
    public async Task<IActionResult> GetTrendingTopicsAsync([FromQuery] int years = 1, [FromQuery] int topCount = 10)
    {
        if (years != 1 && years != 5 && years != 10)
            return BadRequest(ApiResult<IEnumerable<TrendingTopicDto>>.Failure(new[] { "years must be one of: 1, 5, 10." }));

        if (topCount <= 0 || topCount > 50)
            return BadRequest(ApiResult<IEnumerable<TrendingTopicDto>>.Failure(new[] { "topCount must be between 1 and 50." }));

        var result = await _analyticsService.GetTrendingTopicsAsync(years, topCount);
        return Ok(ApiResult<IEnumerable<TrendingTopicDto>>.Success(result));
    }

    /// <summary>
    /// Returns personalized dashboard stats for the authenticated Researcher.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetResearcherDashboardAsync()
    {
        var userIdString = _claimService.GetUserId();
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest(ApiResult<ResearcherDashboardDto>.Failure(new[] { "Invalid user ID." }));

        var result = await _analyticsService.GetResearcherDashboardAsync(userId);
        return Ok(ApiResult<ResearcherDashboardDto>.Success(result));
    }

    #endregion
}
