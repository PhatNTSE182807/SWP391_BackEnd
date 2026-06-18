using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Dashboard;
using N_Tier.Application.Services;
using N_Tier.Shared.Services;

namespace N_Tier.API.Controllers;

[Authorize]
[Tags("Dashboard")]
public class DashboardController : ApiController
{
    private readonly IDashboardService _dashboardService;
    private readonly IClaimService _claimService;

    public DashboardController(IDashboardService dashboardService, IClaimService claimService)
    {
        _dashboardService = dashboardService;
        _claimService = claimService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        var userIdString = _claimService.GetUserId();
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest(ApiResult<DashboardSummaryDto>.Failure(new[] { "Invalid user ID" }));

        var summary = await _dashboardService.GetSummaryAsync(userId);
        return Ok(ApiResult<DashboardSummaryDto>.Success(summary));
    }

    [HttpGet("publication-trends")]
    public async Task<IActionResult> GetPublicationTrendsAsync()
    {
        var trends = await _dashboardService.GetPublicationTrendsAsync(6);
        return Ok(ApiResult<IEnumerable<PublicationTrendDto>>.Success(trends));
    }

    [HttpGet("hot-topics")]
    public async Task<IActionResult> GetHotTopicsAsync()
    {
        var hotTopics = await _dashboardService.GetHotTopicsAsync(7);
        return Ok(ApiResult<IEnumerable<HotTopicDto>>.Success(hotTopics));
    }
}
