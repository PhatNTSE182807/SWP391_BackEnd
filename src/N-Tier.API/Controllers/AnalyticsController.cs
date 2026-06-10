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
public class AnalyticsController : ApiController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IClaimService _claimService;

    public AnalyticsController(IAnalyticsService analyticsService, IClaimService claimService)
    {
        _analyticsService = analyticsService;
        _claimService = claimService;
    }

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
    /// Returns Top 10 topics with highest publication growth in the latest available period.
    /// </summary>
    [HttpGet("trending-topics")]
    public async Task<IActionResult> GetTrendingTopicsAsync()
    {
        var result = await _analyticsService.GetTrendingTopicsAsync(10);
        return Ok(ApiResult<IEnumerable<TrendingTopicDto>>.Success(result));
    }

    /// <summary>
    /// Returns personalized dashboard stats for the authenticated Researcher.
    /// </summary>
    [Authorize]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetResearcherDashboardAsync()
    {
        var userIdString = _claimService.GetUserId();
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest(ApiResult<ResearcherDashboardDto>.Failure(new[] { "Invalid user ID." }));

        var result = await _analyticsService.GetResearcherDashboardAsync(userId);
        return Ok(ApiResult<ResearcherDashboardDto>.Success(result));
    }
}
